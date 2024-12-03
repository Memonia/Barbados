using System;
using System.Text;

using Barbados.StorageEngine.Documents.Serialisation;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public sealed class Builder
		{
			private ObjectId _id;
			private readonly RadixTreeBuffer.Builder _builder;

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

			public Builder AddDocumentFrom(BarbadosIdentifier field, BarbadosDocument document)
			{
				BarbadosArgumentException.ThrowFieldIdentifierWhenDocumentExpected(field, nameof(field));
				if (!document.Buffer.TryExtract(field.BinaryName.AsBytes(), out var buffer))
				{
					throw new ArgumentException(
						$"Given document did not contain a document identifier '{field}'", nameof(field)
					);
				}

				var sb = new StringBuilder();
				var e = buffer.GetKeyValueEnumerator();
				while (e.TryGetNext(out var key, out var valueBuffer))
				{
					sb.Append(field).Append(key);
					var cat = sb.ToString();
					_builder.AddBuffer(cat, valueBuffer);
				}

				return this;
			}

			public Builder AddFieldFrom(BarbadosIdentifier field, BarbadosDocument document)
			{
				BarbadosArgumentException.ThrowDocumentIdentifierWhenFieldExpected(field, nameof(field));
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
					if (!document.Buffer.TryGetBuffer(field.BinaryName.AsBytes(), out var valueBuffer))
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
				BarbadosArgumentException.ThrowDocumentIdentifierWhenFieldExpected(field, nameof(field));
				if (document.Count() == 0)
				{
					throw new ArgumentException("Cannot add an empty document", nameof(document));
				}

				var sb = new StringBuilder($"{field}{CommonIdentifiers.NestingSeparator}");
				var e = document.Buffer.GetKeyValueEnumerator();
				while (e.TryGetNext(out var key, out var valueBuffer))
				{
					sb.Append(key);
					var f = sb.ToString();
					sb.Length = field.Identifier.Length + 1;

					_builder.AddBuffer(f, valueBuffer);
				}

				// Underlying radix tree has no concept of nested documents, as it only operates
				// on prefixes. 'BarbadosDocument' adds document semantincs by enforcing specific
				// naming conventions. For example, key 'pet' refers to a field, key 'pet.nickname'
				// refers to a field 'nickname' inside of the 'pet' document and so on.
				//
				// For the radix tree, however, both of those strings are just node paths, which
				// can be traversed by receiving the same string as was used during the creation of
				// the path.
				//
				// Consider this example:
				// {
				//   "pet": {
				//     "nickname": "Fluffy"
				//   }
				// }
				// 
				// Disregarding specific serialisation details, the radix tree will contain a single
				// node with prefix 'pet.nickname'. Retrieving 'pet.nickname' works as expected. Now,
				// if we were to retrieve the whole document, we would try document.GetDocument("pet")
				// or document.GetDocument("pet.") and both of these would fail, as there is no node
				// path corresponding to strings "pet" or "pet.", so the root for breadth-first
				// traversal cannot be established. 
				// 
				// In fact, any sequence of keys, where each next key contains the previous, would
				// make it impossible to extract the whole document, while each individual key would
				// be accessibile as usual: pet.nickname, pet.nicknamenickname,
				// pet.nicknamenicknamenickname, pet.nicknamenicknamenicknamenickname, etc
				// 
				// To enforce desired behaviour, we explicitly insert a prefix corresponding to the
				// document key with no value. This will ensure the document key is addressable and
				// can serve as a root for extract operations.
				// 
				// This trick simply exploits the fact that the radix tree keeps any added prefix
				// addressable, regardless of whether it has a value. Added overhead is currently
				// 4 bytes for the prefix descriptor, the prefix chain itself remains unchanged
				if (!_builder.PrefixExists(sb.ToString()))
				{
					_builder.AddPrefix(sb.ToString());
				}

				return this;
			}

			public Builder Add(BarbadosIdentifier field, BarbadosDocument[] array)
			{
				if (array.Length == 0)
				{
					throw new ArgumentException("Cannot add an empty array", nameof(array));
				}

				var sb = new StringBuilder($"{field}{CommonIdentifiers.NestingSeparator}");
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
				BarbadosArgumentException.ThrowDocumentIdentifierWhenFieldExpected(field, nameof(field));
				add();
				return this;
			}
		}
	}
}
