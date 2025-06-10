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
				var set = new HashSet<BarbadosKey>(keys);
				var builder = new RadixTreeBuffer.Builder();
				foreach (var key in set)
				{
					if (key.IsDocument)
					{
						var e = new RadixTreeBuffer.PrefixValueEnumerator(bytes, key.SearchPrefix);
						while (e.TryGetNext(out var sk, out var valueBuffer))
						{
							builder.AddBuffer(sk, valueBuffer);
						}
					}

					else
					{
						if (RadixTreeBuffer.TryGetBuffer(bytes, key.SearchPrefix, out var valueBuffer))
						{
							builder.AddBuffer(key.SearchPrefix, valueBuffer);
						}
					}
				}

				return new(builder.Build());
			}

			public static BarbadosDocument FromBytesExclude(ReadOnlySpan<byte> bytes, IEnumerable<BarbadosKey> keys)
			{
				var include = new HashSet<BarbadosKey>();
				var e = new RadixTreeBuffer.PrefixValueEnumerator(bytes);
				while (e.TryGetNext(out var key, out _))
				{
					include.Add(new(key));
				}

				foreach (var key in keys)
				{
					include.Remove(key);
				}

				return FromBytesInclude(bytes, include);
			}
		}
	}
}
