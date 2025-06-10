using System;
using System.Text;

using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Helpers
{
	internal static class FormatHelpers
	{
		private static readonly int _newLineLength = Environment.NewLine.Length;

		public static string FormatPlan(IQueryPlan plan)
		{
			static void _format(StringBuilder builder, IQueryPlan root, int indent)
			{
				builder.Append(' ', indent * 2);
				builder.Append(root.ToString());
				if (root.Child is not null)
				{
					builder.AppendLine();
					_format(builder, root.Child, indent + 1);
				}
			}

			var sb = new StringBuilder();
			_format(sb, plan, 0);
			return sb.ToString();
		}

		public static string FormatPlanEvaluator(IQueryPlanEvaluator evaluator)
		{
			static void _format(StringBuilder builder, IQueryPlanEvaluator root, int indent)
			{
				var rootStr = root.ToString();
				var rootStrSpan = rootStr.AsSpan();
				foreach (var line in rootStrSpan.Split(Environment.NewLine))
				{
					builder.Append(' ', indent * 2);
					builder.Append(rootStrSpan[line]);
					builder.AppendLine();
				}

				foreach (var child in root.Children)
				{
					_format(builder, child, indent + 1);
				}
			}

			var sb = new StringBuilder();
			_format(sb, evaluator, 0);
			return sb.ToString();
		}

		public static string FormatSelection(string name, FindOptions options)
		{
			var sb = new StringBuilder();
			sb.Append($"{name}: ");
			sb.Append(options.AsDocument());
			return sb.ToString();
		}

		public static string FormatSelection(string name, KeySelection selection)
		{
			if (selection.Keys.Count == 0)
			{
				return $"{name}: [?all]";
			}

			var sb = new StringBuilder();
			foreach (var projection in selection.Keys)
			{
				sb.Append($"'{projection}'");
				sb.Append(", ");
			}

			if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 2, 2);
			}

			return $"{name}: ?{(selection.KeysIncluded ? "include" : "exclude")} [{sb}]";
		}
	}
}
