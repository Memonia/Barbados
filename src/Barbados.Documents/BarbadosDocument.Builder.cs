using System;
using System.Linq;
using System.Text;

using Barbados.Documents.Exceptions;
using Barbados.Documents.RadixTree;
using Barbados.Documents.RadixTree.Exceptions;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public sealed partial class Builder
		{
			private readonly RadixTreeBuffer.Builder _builder;

			public Builder()
			{
				_builder = new();
			}

			public BarbadosDocument Build(bool reset = true)
			{
				var buf = _builder.Build();
				var doc = new BarbadosDocument(buf);
				if (reset)
				{
					_builder.Reset();
				}

				return doc;
			}

			public Builder AddFrom(BarbadosDocument document)
			{
				var e = document.GetKeyEnumerator(flat: false);
				while (e.MoveNext())
				{
					var current = e.GetCurrent();
					Add(current, document.Get(current));
				}

				return this;
			}

			public Builder AddFrom(BarbadosKey key, BarbadosDocument document)
			{
				if (document.TryGet(key, out var value))
				{
					Add(key, value);
					return this;
				}

				throw new ArgumentException(
					$"Given document did not contain a key '{key}'", nameof(key)
				);
			}

			public Builder Add(BarbadosKey key, object value)
			{
				var t = value.GetType();
				return t switch
				{
					_ when t == typeof(sbyte) => Add(key, (sbyte)value),
					_ when t == typeof(short) => Add(key, (short)value),
					_ when t == typeof(int) => Add(key, (int)value),
					_ when t == typeof(long) => Add(key, (long)value),
					_ when t == typeof(byte) => Add(key, (byte)value),
					_ when t == typeof(ushort) => Add(key, (ushort)value),
					_ when t == typeof(uint) => Add(key, (uint)value),
					_ when t == typeof(ulong) => Add(key, (ulong)value),
					_ when t == typeof(float) => Add(key, (float)value),
					_ when t == typeof(double) => Add(key, (double)value),
					_ when t == typeof(bool) => Add(key, (bool)value),
					_ when t == typeof(DateTime) => Add(key, (DateTime)value),
					_ when t == typeof(string) => Add(key, (string)value),
					_ when t == typeof(sbyte[]) => Add(key, (sbyte[])value),
					_ when t == typeof(short[]) => Add(key, (short[])value),
					_ when t == typeof(int[]) => Add(key, (int[])value),
					_ when t == typeof(long[]) => Add(key, (long[])value),
					_ when t == typeof(byte[]) => Add(key, (byte[])value),
					_ when t == typeof(ushort[]) => Add(key, (ushort[])value),
					_ when t == typeof(uint[]) => Add(key, (uint[])value),
					_ when t == typeof(ulong[]) => Add(key, (ulong[])value),
					_ when t == typeof(float[]) => Add(key, (float[])value),
					_ when t == typeof(double[]) => Add(key, (double[])value),
					_ when t == typeof(bool[]) => Add(key, (bool[])value),
					_ when t == typeof(DateTime[]) => Add(key, (DateTime[])value),
					_ when t == typeof(string[]) => Add(key, (string[])value),
					_ when t == typeof(BarbadosDocument) => Add(key, (BarbadosDocument)value),
					_ when t == typeof(BarbadosDocument[]) => Add(key, (BarbadosDocument[])value),
					_ => throw new ArgumentException($"Unsupported type '{t}'", nameof(value)),
				};
			}

			public Builder Add(BarbadosKey key, sbyte value) => _addValue(key, () => _builder.AddInt8(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, short value) => _addValue(key, () => _builder.AddInt16(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, int value) => _addValue(key, () => _builder.AddInt32(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, long value) => _addValue(key, () => _builder.AddInt64(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, byte value) => _addValue(key, () => _builder.AddUInt8(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, ushort value) => _addValue(key, () => _builder.AddUInt16(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, uint value) => _addValue(key, () => _builder.AddUInt32(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, ulong value) => _addValue(key, () => _builder.AddUInt64(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, float value) => _addValue(key, () => _builder.AddFloat32(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, double value) => _addValue(key, () => _builder.AddFloat64(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, bool value) => _addValue(key, () => _builder.AddBoolean(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, DateTime value) => _addValue(key, () => _builder.AddDateTime(key.ValueSearchPrefix, value));
			public Builder Add(BarbadosKey key, string value) => _addValue(key, () => _builder.AddString(key.ValueSearchPrefix, value));

			public Builder Add(BarbadosKey key, sbyte[] array) => _addValue(key, () => _builder.AddInt8Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, short[] array) => _addValue(key, () => _builder.AddInt16Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, int[] array) => _addValue(key, () => _builder.AddInt32Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, long[] array) => _addValue(key, () => _builder.AddInt64Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, byte[] array) => _addValue(key, () => _builder.AddUInt8Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, ushort[] array) => _addValue(key, () => _builder.AddUInt16Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, uint[] array) => _addValue(key, () => _builder.AddUInt32Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, ulong[] array) => _addValue(key, () => _builder.AddUInt64Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, float[] array) => _addValue(key, () => _builder.AddFloat32Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, double[] array) => _addValue(key, () => _builder.AddFloat64Array(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, bool[] array) => _addValue(key, () => _builder.AddBooleanArray(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, DateTime[] array) => _addValue(key, () => _builder.AddDateTimeArray(key.ValueSearchPrefix, array));
			public Builder Add(BarbadosKey key, string[] array) => _addValue(key, () => _builder.AddStringArray(key.ValueSearchPrefix, array));

			public Builder Add(BarbadosKey key, BarbadosDocument document)
			{
				if (document.Count() == 0)
				{
					throw new ArgumentException("Cannot add an empty document", nameof(document));
				}

				if (_builder.PrefixExists(key.ValueSearchPrefix) || _builder.PrefixExists(key.DocumentSearchPrefix))
				{
					BarbadosArgumentExceptionHelpers.ThrowKeyAlreadyExists(key.ToString(), nameof(key));
				}

				var sb = new StringBuilder(key.ToString());
				var startLength = sb.Length;
				var e = document.GetKeyEnumerator(flat: true);
				while (e.MoveNext())
				{
					var current = e.GetCurrent();
					sb.Append(BarbadosKey.NestingSeparator);
					sb.Append(current);
					var nf = sb.ToString();
					sb.Length = startLength;
					Add(nf, document.Get(current));
				}

				_ensureNestingPathAddressable(key.DocumentSearchPrefix);
				return this;
			}

			public Builder Add(BarbadosKey key, params BarbadosDocument[] array)
			{
				if (array.Length == 0)
				{
					throw new ArgumentException("Cannot add an empty array", nameof(array));
				}

				if (array.Any(e => e.Count() == 0))
				{
					throw new ArgumentException(
						"An array of documents cannot contain empty documents", nameof(array)
					);
				}

				if (_builder.PrefixExists(key.ValueSearchPrefix) || _builder.PrefixExists(key.DocumentSearchPrefix))
				{
					BarbadosArgumentExceptionHelpers.ThrowKeyAlreadyExists(key.ToString(), nameof(key));
				}

				var sb = new StringBuilder(key.ToString());
				sb.Append(BarbadosKey.NestingSeparator);
				var startLength = sb.Length;
				for (int i = 0; i < array.Length; ++i)
				{
					var document = array[i];
					sb.Append(i);

					BarbadosKey nf = sb.ToString();
					sb.Length = startLength;

					Add(nf, document);
					_ensureNestingPathAddressable(nf.DocumentSearchPrefix);
				}

				_ensureNestingPathAddressable(key.DocumentSearchPrefix);
				return this;
			}

			internal Builder Add(RadixTreePrefixSpan prefix, IValueBuffer buffer)
			{
				_ensureNestingPathAddressable(prefix);
				_builder.AddBuffer(prefix, buffer);
				return this;
			}

			private Builder _addValue(BarbadosKey key, Action add)
			{
				if (_builder.PrefixExists(key.DocumentSearchPrefix))
				{
					BarbadosArgumentExceptionHelpers.ThrowKeyAlreadyExists(key.ToString(), nameof(key));
				}


				try
				{
					add();
				}

				catch (RadixTreeDuplicateKeyException ex)
				{
					BarbadosArgumentExceptionHelpers.ThrowKeyAlreadyExists(key.ToString(), nameof(key), ex);
				}

				_ensureNestingPathAddressable(key.ValueSearchPrefix);
				return this;
			}

			private void _ensureNestingPathAddressable(RadixTreePrefixSpan prefix)
			{
				// Underlying radix tree has no concept of nested documents, as it only operates
				// on prefixes. 'BarbadosDocument' adds document semantics by enforcing specific
				// naming conventions. For example, key 'pet' refers to a key, key 'pet.nickname'
				// refers to a key 'nickname' inside of the 'pet' document and so on.
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

				var kbytes = prefix.AsBytes();
				var nestSepBytes = BarbadosKey.NestingSeparatorAsPrefix.AsBytes();
				var nestSepLength = BarbadosKey.NestingSeparatorAsPrefix.Length;
				if (kbytes.IndexOf(nestSepBytes) < 0)
				{
					return;
				}

				var split = kbytes.Split(nestSepBytes);
				foreach (var subKeyRange in split)
				{
					var subKey = new RadixTreePrefixSpan(kbytes[0..subKeyRange.End]);

					// A key cannot be both a value and a document.
					// Check any ancestor prefix already holds a value
					if (subKey.Length < kbytes.Length && _builder.PrefixHasValue(subKey))
					{
						BarbadosArgumentExceptionHelpers.ThrowKeyConflictsWithAncestor(
							prefix.ToString(), subKey.ToString(), "key"
						);
					}

					if (subKey.Length + nestSepLength < kbytes.Length)
					{
						subKey = new RadixTreePrefixSpan(kbytes[0..(subKeyRange.End.Value + nestSepLength)]);
					}

					if (!_builder.PrefixExists(subKey))
					{
						_builder.AddPrefix(subKey);
					}
				}
			}
		}
	}
}
