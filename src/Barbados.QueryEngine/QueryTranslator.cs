using System;
using System.Collections.Generic;
using System.Linq;

using Barbados.Documents;
using Barbados.QueryEngine.Build;
using Barbados.QueryEngine.Build.Expressions;
using Barbados.QueryEngine.Evaluation;
using Barbados.QueryEngine.Evaluation.Expressions;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine
{
	internal static class QueryTranslator
	{
		/* Currently, we're able to substitute collection scans with index seeks given 
		 * the simplest expressions, only examining the closest filter to the scan
		 */

		public static IQueryPlanEvaluator TranslatePlan(
			IQueryPlan plan,
			IReadOnlyBarbadosCollection collection
		)
		{
			IReadOnlyCollection<IQueryPlanEvaluator> _translate(IQueryPlan child, List<IQueryExpression> filterExpressions)
			{
				if (child is Scan scan)
				{
					if (filterExpressions.Count > 0)
					{
						var expr = filterExpressions[^1];
						if (_tryGetMatchingIndexSeekEvaluator(expr, collection, scan.Selector, out var ieval))
						{
							return [ieval];
						}
					}

					var eval = new CollectionScanEvaluator(collection, scan.Selector);
					return [eval];
				}

				else
				if (child is Filter filter)
				{
					filterExpressions.Add(filter.Expression);
					var expr = TranslateExpression(filter.Expression);
					var input = _translate(filter.Child, filterExpressions);
					var filterEvaluator = new FilterEvaluator(input, expr);
					return [filterEvaluator];
				}

				else
				if (child is Projection projection)
				{
					var input = _translate(projection.Child, filterExpressions);
					var projectionEvaluator = new ProjectionEvaluator(input, projection.Selector, new());
					return [projectionEvaluator];
				}

				else
				{
					throw new NotImplementedException();
				}
			}

			return _translate(plan, []).First();
		}

		public static IQueryExpressionEvaluator TranslateExpression(IQueryExpression expr)
		{
			if (expr is NotExpression ne)
			{
				var eval = TranslateExpression(ne.Expression);
				return new NotExpressionEvaluator(ne, eval, new());
			}

			else
			if (expr is BinaryExpression be)
			{
				if (expr is ComparisonExpression ce)
				{
					return ComparisonExpressionEvaluatorFactory.Create(
						ce,
						ce.ComparedField,
						ce.Operator,
						TranslateExpression(ce.Left),
						TranslateExpression(ce.Right)
					);
				}

				return BinaryExpressionEvaluatorFactory.Create(
					be,
					be.Operator,
					TranslateExpression(be.Left),
					TranslateExpression(be.Right)
				);
			}

			else
			if (expr is FieldExpression fe)
			{
				return new FieldExpressionEvaluator(fe);
			}

			else
			if (expr.GetType().GetGenericTypeDefinition() == typeof(ConstantExpression<>))
			{
				var builder = new BarbadosDocument.Builder();
				return expr switch
				{
					ConstantExpression<sbyte> cesb => new ConstantExpressionEvaluator(expr, builder.Add(cesb.Name, cesb.Value).Build()),
					ConstantExpression<int> cei => new ConstantExpressionEvaluator(expr, builder.Add(cei.Name, cei.Value).Build()),
					ConstantExpression<short> ces => new ConstantExpressionEvaluator(expr, builder.Add(ces.Name, ces.Value).Build()),
					ConstantExpression<long> cel => new ConstantExpressionEvaluator(expr, builder.Add(cel.Name, cel.Value).Build()),
					ConstantExpression<byte> ceb => new ConstantExpressionEvaluator(expr, builder.Add(ceb.Name, ceb.Value).Build()),
					ConstantExpression<uint> ceui => new ConstantExpressionEvaluator(expr, builder.Add(ceui.Name, ceui.Value).Build()),
					ConstantExpression<ushort> ceus => new ConstantExpressionEvaluator(expr, builder.Add(ceus.Name, ceus.Value).Build()),
					ConstantExpression<ulong> ceul => new ConstantExpressionEvaluator(expr, builder.Add(ceul.Name, ceul.Value).Build()),
					ConstantExpression<float> cef => new ConstantExpressionEvaluator(expr, builder.Add(cef.Name, cef.Value).Build()),
					ConstantExpression<double> ced => new ConstantExpressionEvaluator(expr, builder.Add(ced.Name, ced.Value).Build()),
					ConstantExpression<DateTime> cedt => new ConstantExpressionEvaluator(expr, builder.Add(cedt.Name, cedt.Value).Build()),
					ConstantExpression<bool> ceb => new ConstantExpressionEvaluator(expr, builder.Add(ceb.Name, ceb.Value).Build()),
					ConstantExpression<string> ces => new ConstantExpressionEvaluator(expr, builder.Add(ces.Name, ces.Value).Build()),
					_ => throw new ArgumentException($"Constant expression of type {expr.GetType().GetGenericArguments()[0]} is not supported")
				};
			}

			else
			{
				throw new NotImplementedException();
			}
		}

		private static bool _tryGetMatchingIndexSeekEvaluator(
			IQueryExpression expression, 
			IReadOnlyBarbadosCollection collection, 
			BarbadosKeySelector selector,
			out IndexSeekEvaluator evaluator
		)
		{
			if (expression is ComparisonExpression ce)
			{
				if (ce.Left is not FieldExpression && ce.Right is not FieldExpression)
				{
					evaluator = default!;
					return false;
				}

				var expr = ce.Left is FieldExpression ? ce.Right : ce.Left;
				if (expr.GetType().GetGenericTypeDefinition() != typeof(ConstantExpression<>))
				{
					evaluator = default!;
					return false;
				}

				if (!collection.TryGetBTreeIndex(ce.ComparedField, out var index))
				{
					evaluator = default!;
					return false;
				}

				var conditionBuilder = new BarbadosDocument.Builder();
				switch (ce.Operator)
				{
					case BinaryOperator.Equals:
						conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.Exact, true);
						break;

					case BinaryOperator.LessThan:
						conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true);
						break;

					case BinaryOperator.GreaterThan:
						conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true);
						break;

					case BinaryOperator.LessThanOrEqual:
						conditionBuilder
							.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
							.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true);
						break;

					case BinaryOperator.GreaterThanOrEqual:
						conditionBuilder
							.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
							.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true);
						break;

					default:
						evaluator = default!;
						return false;
				}

				_addSearchValueToIndexCondition(conditionBuilder, expr);
				evaluator = new IndexSeekEvaluator(selector, conditionBuilder.Build(), index, collection);
				return true;
			}

			evaluator = default!;
			return false;
		}

		private static void _addSearchValueToIndexCondition(
			BarbadosDocument.Builder conditionBuilder,
			IQueryExpression expression
		)
		{
			switch (expression)
			{
				case ConstantExpression<sbyte> cesb:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, cesb.Value);
					break;

				case ConstantExpression<int> cei:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, cei.Value);
					break;

				case ConstantExpression<short> ces:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ces.Value);
					break;

				case ConstantExpression<long> cel:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, cel.Value);
					break;

				case ConstantExpression<byte> ceb:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ceb.Value);
					break;

				case ConstantExpression<uint> ceui:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ceui.Value);
					break;

				case ConstantExpression<ushort> ceus:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ceus.Value);
					break;

				case ConstantExpression<ulong> ceul:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ceul.Value);
					break;

				case ConstantExpression<float> cef:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, cef.Value);
					break;

				case ConstantExpression<double> ced:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ced.Value);
					break;

				case ConstantExpression<DateTime> cedt:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, cedt.Value);
					break;

				case ConstantExpression<bool> ceb:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ceb.Value);
					break;

				case ConstantExpression<string> ces:
					conditionBuilder.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, ces.Value);
					break;

				default:
					throw new ArgumentException(
						$"Constant expression of type {expression.GetType().GetGenericArguments()[0]} is not supported"
					);
			}
		}
	}
}
