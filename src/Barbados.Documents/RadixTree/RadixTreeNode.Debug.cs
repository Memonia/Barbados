using System.Text;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeNode
	{
		public string Format()
		{
			var sb = new StringBuilder();

			_format(sb, 0);
			return sb.ToString();
		}

		private void _format(StringBuilder sb, int depth)
		{
			sb.Append(' ');
			sb.Append(Value?.ToString() ?? "<none>");
			sb.AppendLine();
			foreach (var (prefix, child) in _children)
			{
				sb.Append(' ', depth);
				sb.Append('|');
				sb.Append(prefix);
				child._format(sb, depth + 1);
			}
		}
	}
}
