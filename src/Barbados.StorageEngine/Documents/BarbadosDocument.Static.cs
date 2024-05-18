using System.Collections;
using System.Diagnostics;
using System.Text;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public static readonly BarbadosDocument Empty;

		static BarbadosDocument()
		{
			Empty = new Builder().Build();
		}

		public static bool TryCompareFields(BarbadosIdentifier field, BarbadosDocument a, BarbadosDocument b, out int result)
		{
			if (!a.Buffer.TryGetBufferValueBytesRaw(field.StringBufferValue, out var ma, out var rawa))
			{
				result = default!;
				return false;
			}

			if (!b.Buffer.TryGetBufferValueBytesRaw(field.StringBufferValue, out var mb, out var rawb))
			{
				result = default!;
				return false;
			}

			if (ma != mb)
			{
				result = default!;
				return false;
			}

			var comparer = ValueSpanComparerFactory.GetComparer(ma);
			result = comparer.Compare(rawa, rawb);
			return true;
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
				builder.AppendFormat(format, "?id", document.Id);
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
