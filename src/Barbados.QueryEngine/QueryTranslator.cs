using System;
using System.Collections.Generic;
using System.Linq;

using Barbados.Documents;
using Barbados.QueryEngine.Build;
using Barbados.QueryEngine.Build.Expressions;
using Barbados.QueryEngine.Evaluation;
using Barbados.QueryEngine.Evaluation.Expressions;
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
				if (child is ScanPlan scan)
				{
					var fob = new FindOptionsBuilder();
					if (!scan.Selection.SelectAll)
					{
						fob = scan.Selection.KeysIncluded
							? fob.Include([.. scan.Selection.Keys.Select(e => new BarbadosKey(e))])
							: fob.Exclude([.. scan.Selection.Keys.Select(e => new BarbadosKey(e))]);
					}

					if (filterExpressions.Count > 0)
					{
						var expr = filterExpressions[^1];
						if (_tryGetMatchingIndexSeekEvaluator(expr, collection, fob, out var ieval))
						{
							return [ieval];
						}
					}


					var eval = new CollectionScanEvaluator(collection, fob.Build());
					return [eval];
				}

				else
				if (child is FilterPlan filter)
				{
					filterExpressions.Add(filter.Expression);
					var expr = TranslateExpression(filter.Expression);
					var input = _translate(filter.Child, filterExpressions);
					var filterEvaluator = new FilterEvaluator(input, expr);
					return [filterEvaluator];
				}

				else
				if (child is ProjectionPlan projection)
				{
					var input = _translate(projection.Child, filterExpressions);
					var projectionEvaluator = new ProjectionEvaluator(input, projection.Selection, new());
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
			FindOptionsBuilder findOptionsWithProjection,
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

				if (!collection.IndexExists(ce.ComparedField))
				{
					evaluator = default!;
					return false;
				}

				object value = expression switch
				{
					ConstantExpression<sbyte> cesb => cesb.Value,
					ConstantExpression<int> cei => cei.Value,
					ConstantExpression<short> ces => ces.Value,
					ConstantExpression<long> cel => cel.Value,
					ConstantExpression<byte> ceb => ceb.Value,
					ConstantExpression<uint> ceui => ceui.Value,
					ConstantExpression<ushort> ceus => ceus.Value,
					ConstantExpression<ulong> ceul => ceul.Value,
					ConstantExpression<float> cef => cef.Value,
					ConstantExpression<double> ced => ced.Value,
					ConstantExpression<DateTime> cedt => cedt.Value,
					ConstantExpression<bool> ceb => ceb.Value,
					ConstantExpression<string> ces => ces.Value,
					_ => throw new ArgumentException($"Constant expression of type {expression.GetType().GetGenericArguments()[0]} is not supported")
				};

				var fob = findOptionsWithProjection;
				fob = ce.Operator switch
				{
					BinaryOperator.Equals => fob.Eq(value),
					BinaryOperator.LessThan => fob.Lt(value),
					BinaryOperator.GreaterThan => fob.Gt(value),
					BinaryOperator.LessThanOrEqual => fob.LtEq(value),
					BinaryOperator.GreaterThanOrEqual => fob.GtEq(value),
					_ => throw new ArgumentException($"Operator {ce.Operator} is not supported")

				};

				evaluator = new IndexSeekEvaluator(collection, ((FieldExpression)expr).Name, fob.Build());
				return true;
			}

			evaluator = default!;
			return false;
		}
	}
}
