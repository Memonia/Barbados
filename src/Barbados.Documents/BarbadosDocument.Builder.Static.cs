using System;
using System.Collections.Generic;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public partial class Builder
		{
			public static BarbadosDocument FromBytes(byte[] bytes)
			{
				var buffer = new RadixTreeBuffer(bytes);
				return new(buffer);
			}

			public static BarbadosDocument FromBytes(ReadOnlySpan<byte> bytes)
			{
				return FromBytes(bytes.ToArray());
			}

			public static BarbadosDocument FromBytesInclude(ReadOnlySpan<byte> bytes, IEnumerable<BarbadosKey> keys)
			{
				// TODO: Any way to reduce allocations?

				if (RadixTreeBuffer.IsEmpty(bytes))
				{
					return BarbadosDocument.Empty;
				}

				var set = new HashSet<BarbadosKey>(keys);
				var builder = new Builder();
				foreach (var key in set)
				{
					if (RadixTreeBuffer.TryGetBuffer(bytes, key.ValueSearchPrefix, out var valueBuffer))
					{
						builder.Add(key.ValueSearchPrefix, valueBuffer);
					}

					else
					{
						var e = new RadixTreeBuffer.FullPrefixHasValueEnumerator(bytes, key.DocumentSearchPrefix);
						while (e.MoveNext())
						{
							var sk = e.GetCurrentPrefix(out valueBuffer);
							var keyPrefixLength = key.DocumentSearchPrefix.AsBytes().Length;
							var full = new byte[keyPrefixLength + sk.AsBytes().Length];
							key.DocumentSearchPrefix.AsBytes().CopyTo(full);
							sk.AsBytes().CopyTo(full.AsSpan()[keyPrefixLength..]);
							builder.Add(new RadixTreePrefixSpan(full), valueBuffer);
						}
					}
				}

				return builder.Build();
			}

			public static BarbadosDocument FromBytesExclude(ReadOnlySpan<byte> bytes, IEnumerable<BarbadosKey> keys)
			{
				// TODO: Implement it properly. Right now it's incredibly lazy and much slower than the 'include'

				if (RadixTreeBuffer.IsEmpty(bytes))
				{
					return BarbadosDocument.Empty;
				}

				var include = new HashSet<BarbadosKey>();
				var doc = FromBytes(bytes);
				var e = doc.GetKeyEnumerator(flat: true);
				while (e.MoveNext())
				{
					include.Add(e.GetCurrent());
				}

				foreach (var key in keys)
				{
					if (doc.TryGetDocumentKeyEnumerator(key, flat: true, out e))
					{
						while (e.MoveNext())
						{
							include.Remove(key.DocumentSearchPrefix.ToString() + e.GetCurrent());
						}
					}
					else
					{
						include.Remove(key);
					}
				}

				return FromBytesInclude(bytes, include);
			}
		}
	}
}
