using System;
using System.Collections.Generic;

using Barbados.Documents.Serialisation;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public partial class Builder
		{
			public static BarbadosDocument FromBytes(ReadOnlySpan<byte> bytes)
			{
				var buffer = new RadixTreeBuffer(bytes.ToArray());
				return new(buffer);
			}

			public static BarbadosDocument FromBytes(ReadOnlySpan<byte> bytes, IEnumerable<BarbadosKey> select)
			{
				var builder = new RadixTreeBuffer.Builder();
				foreach (var key in select)
				{			
					if (key.IsDocument)
					{
						var e = new RadixTreeBuffer.PrefixValueEnumerator(bytes);
						while (e.TryGetNext(out var sk, out var value))
						{
							builder.AddBuffer(sk, value);
						}
					}

					else
					{
						if (RadixTreeBuffer.TryGetBuffer(bytes, key.SearchPrefix.AsBytes(), out var valueBuffer))
						{
							var prefix = new RadixTreePrefix(key.ToString());
							builder.AddBuffer(prefix, valueBuffer);
						}
					}
				}

				return new(builder.Build());
			}
		}
	}
}
