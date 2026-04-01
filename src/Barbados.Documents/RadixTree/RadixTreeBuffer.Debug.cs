using System.Text;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		public string FormatPrefixTable()
		{
			var sb = new StringBuilder();
			var vto = _getValueTableOffset(_buffer);
			var i = PrefixTableOffset;
			do
			{
				var pd = _getPrefixDescriptor(_buffer, i);
				var info = new NodeInfo(i, pd);
				var prefix = new RadixTreePrefixSpan(_getNodePrefix(_buffer, info));
				sb.Append('[');
				sb.Append($"LC: {(pd.IsLastChild ? 1 : 0)}");
				sb.Append(' ');
				sb.Append($"HC: {(pd.HasChildren ? 1 : 0)}");
				sb.Append(' ');
				sb.Append($"HV: {(pd.HasValue ? 1 : 0)}");
				sb.Append(']');
				sb.Append(' ');
				sb.Append(prefix.ToString());
				sb.AppendLine();

				i = _getNextNodeOffset(_buffer, info);
			} while (i < vto);

			return sb.ToString();
		}

		public string FormatPrefixTableAsTree()
		{
			void _format(StringBuilder sb, NodeInfo info, int level)
			{
				var tab = new string('|', level);
				var prefix = new RadixTreePrefixSpan(_getNodePrefix(_buffer, info));

				sb.Append(tab);
				sb.AppendLine(prefix.ToString());
				if (info.Descriptor.HasChildren)
				{
					var i = _getNodeFirstChildOffset(_buffer, info.Descriptor);
					var pd = _getPrefixDescriptor(_buffer, i);
					_format(sb, new(i, pd), level + 1);
				}

				if (!info.Descriptor.IsLastChild)
				{
					var i = _getNextNodeOffset(_buffer, info);
					var pd = _getPrefixDescriptor(_buffer, i);
					_format(sb, new(i, pd), level);
				}
			}

			var sb = new StringBuilder();
			var vto = _getValueTableOffset(_buffer);
			var i = PrefixTableOffset;
			var pd = _getPrefixDescriptor(_buffer, i);
			var info = new NodeInfo(i, pd);

			_format(sb, info, 1);
			return sb.ToString();
		}
	}
}
