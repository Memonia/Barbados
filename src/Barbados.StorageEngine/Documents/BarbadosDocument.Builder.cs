using System;
using System.Diagnostics;
using System.Text;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public sealed class Builder
		{
			private static void _throwGroupIdentifier(BarbadosIdentifier identifier)
			{
				if (identifier.IsGroup)
				{
					throw new ArgumentException(
						$"Expected a field identifier, got a group '{identifier}'", nameof(identifier)
					);
				}
			}

			private static void _throwFieldIdentifier(BarbadosIdentifier identifier)
			{
				if (!identifier.IsGroup)
				{
					throw new ArgumentException(
						$"Expected a group identifier, got a field '{identifier}'", nameof(identifier)
					);
				}
			}

			private ObjectId _id;
			private readonly ObjectBuffer.Builder _builder;

			public Builder()
			{
				_id = ObjectId.Invalid;
				_builder = new();
			}

			public BarbadosDocument Build(bool reset = true)
			{
				var buf = _builder.Build();
				var doc = new BarbadosDocument(_id, buf);
				if (reset)
				{
					_id = ObjectId.Invalid;
					_builder.Reset();
				}

				return doc;
			}

			public Builder SetId(ObjectId id)
			{
				if (!id.IsValid)
				{
					throw new ArgumentException("Attempted to set an invalid id", nameof(id));
				}

				_id = id;
				return this;
			}

			public Builder AddGroupFrom(BarbadosIdentifier group, BarbadosDocument document)
			{
				_throwFieldIdentifier(group);
				if (!document.Buffer.TryCollect(group.StringBufferValue, false, out var buffer))
				{
					throw new ArgumentException($"Given document did not contain a group '{group}'", nameof(group));
				}

				var e = buffer.GetNameEnumerator();
				while (e.TryGetNext(out var raw, out var name))
				{
					var r = buffer.TryGetBuffer(raw, out var valueBuffer);
					Debug.Assert(r);

					_builder.AddBuffer(name, valueBuffer);
				}

				return this;
			}

			public Builder AddFieldFrom(BarbadosIdentifier field, BarbadosDocument document)
			{
				_throwGroupIdentifier(field);
				if (document.TryGetDocument(field, out var value))
				{
					Add(field, value);
				}

				else
				if (document.TryGetDocumentArray(field, out var array))
				{
					Add(field, array);
				}

				else
				{
					if (!document.Buffer.TryGetBuffer(field.StringBufferValue, out var valueBuffer))
					{
						throw new ArgumentException(
							$"Given document did not contain a field '{field}'", nameof(field)
						);
					}

					_builder.AddBuffer(field, valueBuffer);
				}

				return this;
			}

			public Builder Add(BarbadosIdentifier field, sbyte value) => _add(field, () => _builder.AddInt8(field, value));
			public Builder Add(BarbadosIdentifier field, short value) => _add(field, () => _builder.AddInt16(field, value));
			public Builder Add(BarbadosIdentifier field, int value) => _add(field, () => _builder.AddInt32(field, value));
			public Builder Add(BarbadosIdentifier field, long value) => _add(field, () => _builder.AddInt64(field, value));
			public Builder Add(BarbadosIdentifier field, byte value) => _add(field, () => _builder.AddUInt8(field, value));
			public Builder Add(BarbadosIdentifier field, ushort value) => _add(field, () => _builder.AddUInt16(field, value));
			public Builder Add(BarbadosIdentifier field, uint value) => _add(field, () => _builder.AddUInt32(field, value));
			public Builder Add(BarbadosIdentifier field, ulong value) => _add(field, () => _builder.AddUInt64(field, value));
			public Builder Add(BarbadosIdentifier field, float value) => _add(field, () => _builder.AddFloat32(field, value));
			public Builder Add(BarbadosIdentifier field, double value) => _add(field, () => _builder.AddFloat64(field, value));
			public Builder Add(BarbadosIdentifier field, bool value) => _add(field, () => _builder.AddBoolean(field, value));
			public Builder Add(BarbadosIdentifier field, DateTime value) => _add(field, () => _builder.AddDateTime(field, value));
			public Builder Add(BarbadosIdentifier field, string value) => _add(field, () => _builder.AddString(field, value));

			public Builder Add(BarbadosIdentifier field, sbyte[] array) => _add(field, () => _builder.AddInt8Array(field, array));
			public Builder Add(BarbadosIdentifier field, short[] array) => _add(field, () => _builder.AddInt16Array(field, array));
			public Builder Add(BarbadosIdentifier field, int[] array) => _add(field, () => _builder.AddInt32Array(field, array));
			public Builder Add(BarbadosIdentifier field, long[] array) => _add(field, () => _builder.AddInt64Array(field, array));
			public Builder Add(BarbadosIdentifier field, byte[] array) => _add(field, () => _builder.AddUInt8Array(field, array));
			public Builder Add(BarbadosIdentifier field, ushort[] array) => _add(field, () => _builder.AddUInt16Array(field, array));
			public Builder Add(BarbadosIdentifier field, uint[] array) => _add(field, () => _builder.AddUInt32Array(field, array));
			public Builder Add(BarbadosIdentifier field, ulong[] array) => _add(field, () => _builder.AddUInt64Array(field, array));
			public Builder Add(BarbadosIdentifier field, float[] array) => _add(field, () => _builder.AddFloat32Array(field, array));
			public Builder Add(BarbadosIdentifier field, double[] array) => _add(field, () => _builder.AddFloat64Array(field, array));
			public Builder Add(BarbadosIdentifier field, bool[] array) => _add(field, () => _builder.AddBooleanArray(field, array));
			public Builder Add(BarbadosIdentifier field, DateTime[] array) => _add(field, () => _builder.AddDateTimeArray(field, array));
			public Builder Add(BarbadosIdentifier field, string[] array) => _add(field, () => _builder.AddStringArray(field, array));

			public Builder Add(BarbadosIdentifier field, BarbadosDocument document)
			{
				_throwGroupIdentifier(field);
				if (document.Count() == 0)
				{
					throw new ArgumentException("Cannot add an empty document", nameof(document));
				}

				var sb = new StringBuilder(field);
				var e = document.Buffer.GetNameEnumerator();
				while (e.TryGetNext(out var raw, out var name))
				{
					sb.Append('.');
					sb.Append(name);
					var f = sb.ToString();
					sb.Length = field.Identifier.Length;

					var r = document.Buffer.TryGetBuffer(raw, out var valueBuffer);
					Debug.Assert(r);

					_builder.AddBuffer(f, valueBuffer);
				}

				return this;
			}

			public Builder Add(BarbadosIdentifier field, BarbadosDocument[] array)
			{
				if (array.Length == 0)
				{
					throw new ArgumentException("Cannot add an empty array", nameof(array));
				}

				var sb = new StringBuilder($"{field}.");
				for (int i = 0; i < array.Length; ++i)
				{
					var document = array[i];
					if (document.Count() == 0)
					{
						throw new ArgumentException(
							"An array of documents cannot contain empty documents", nameof(array)
						);
					}

					sb.Append(i);
					var f = sb.ToString();
					sb.Length = field.Identifier.Length + 1;

					Add(f, document);
				}

				return this;
			}

			private Builder _add(BarbadosIdentifier field, Action add)
			{
				_throwGroupIdentifier(field);
				add();
				return this;
			}
		}
	}
}
