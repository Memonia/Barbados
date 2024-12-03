using System.Collections;
using System.Diagnostics;
using System.Text;

using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public static BarbadosDocument Empty { get; }

		static BarbadosDocument()
		{
			Empty = new Builder().Build();
		}

		public static bool TryCompareFields(BarbadosIdentifier field, BarbadosDocument a, BarbadosDocument b, out int result)
		{
			if (a.Buffer.TryGetBufferRaw(field.BinaryName.AsBytes(), out var ma, out var va) &&
				b.Buffer.TryGetBufferRaw(field.BinaryName.AsBytes(), out var mb, out var vb) &&
				ma == mb
			)
			{
				var comparer = ValueBufferSpanComparerFactory.GetComparer(ma);
				result = comparer.Compare(va, vb);
				return true;
			}

			result = 0;
			return false;
		}

		private static void _toString(BarbadosDocument document, StringBuilder builder, int spaces)
		{
			var indent = new string(' ', spaces);
			var format = indent + "  {0}: {1}\n";
			var nestedFormat = indent + "  {0}:\n";

			builder.Append(indent);
			builder.AppendLine("{");

			if (spaces == 0)
			{
				builder.AppendFormat(format, CommonIdentifiers.Id, document.Id);
			}

			foreach (var field in document.GetFields())
			{
				if (document.TryGetDocumentArray(field, out var array))
				{
					builder.AppendFormat(nestedFormat, field);
					builder.Append(indent);
					builder.AppendLine("  [");

					for (int i = 0; i < array.Length; ++i)
					{
						_toString(array[i], builder, spaces + 4);
						if (i < array.Length - 1)
						{
							builder.Append(',');
						}

						builder.AppendLine();
					}

					builder.Append(indent);
					builder.AppendLine("  ]");
				}

				else
				if (document.TryGetDocument(field, out var nested))
				{
					builder.AppendFormat(nestedFormat, field);
					_toString(nested, builder, spaces + 2);
					builder.AppendLine();
				}

				else
				{
					var r = document.TryGet(field, out var value);
					Debug.Assert(r);

					if (value.GetType().IsArray)
					{
						var arr = (IEnumerable)value;
						var sb = new StringBuilder();

						sb.Append('[');
						foreach (var item in arr)
						{
							sb.Append(item);
							sb.Append(", ");
						}

						if (sb.Length >= 2)
						{
							sb.Length -= 2;
						}

						sb.Append(']');
						builder.AppendFormat(format, field, sb);
					}

					else
					{
						builder.AppendFormat(format, field, value);
					}
				}
			}

			builder.Append(indent);
			builder.Append('}');
		}
	}
}
