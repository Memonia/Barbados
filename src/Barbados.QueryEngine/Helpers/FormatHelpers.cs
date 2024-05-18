using System.Text;

using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Helpers
{
	internal static class FormatHelpers
	{
		public static string FormatPlan(IQueryPlan plan)
		{
			static void _format(StringBuilder builder, IQueryPlan root, int indent)
			{
				for (int i = 0; i < indent; ++i)
				{
					builder.Append("  ");
				}

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
				for (int i = 0; i < indent; ++i)
				{
					builder.Append("  ");
				}

				builder.Append(root.ToString());
				foreach (var child in root.Children)
				{
					builder.AppendLine();
					_format(builder, child, indent + 1);
				}
			}

			var sb = new StringBuilder();
			_format(sb, evaluator, 0);
			return sb.ToString();
		}

		public static string FormatValueSelector(string name, ValueSelector selector)
		{
			if (selector.All)
			{
				return $"{name}: [?all]";
			}

			var sb = new StringBuilder();
			foreach (var projection in selector)
			{
				sb.Append($"'{projection}'");
				sb.Append(", ");
			}

			if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 2, 2);
			}

			return $"{name}: [{sb}]";
		}
	}
}
