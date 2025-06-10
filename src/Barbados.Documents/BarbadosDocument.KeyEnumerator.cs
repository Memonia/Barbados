using System;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public ref struct KeyEnumerator
		{
			public BarbadosKey Current { get; private set; }

			private readonly bool _flat;
			private readonly RadixTreeBuffer.PrefixValueEnumerator _enum;
			private ReadOnlySpan<byte> _previousKeyRootDocumentPortion;

			public KeyEnumerator(BarbadosDocument document, bool flat)
			{
				_flat = flat;
				_enum = document._buffer.GetPrefixValueEnumerator();
				_previousKeyRootDocumentPortion = [];
			}

			public bool MoveNext()
			{
				while (true)
				{
					if (!_enum.TryGetNext(out var key))
					{
						return false;
					}

					if (_flat)
					{
						Current = new(key);
						break;
					}

					// We know that prefix enumerators will return prefixes with the same root
					// together, because internally they enumerate nodes in depth-first order.
					// We can take advantage of that and skip duplicates by remembering the
					// previous prefix root.
					//
					// In terms of document keys and values, sequences would look like this:
					//
					// document.field1
					// document.field2
					// document.field3
					// secondDocument.field1
					// secondDocument.field2
					// thirdDocument.field1
					//
					// Thus, we only need to remember the previous root (top level document name) in order
					// to avoid duplicate top level keys
					var rdp = BarbadosKey.GetRootDocumentPortion(key.AsSpan());
					if (!_previousKeyRootDocumentPortion.StartsWith(rdp))
					{
						_previousKeyRootDocumentPortion = rdp;
						Current = new(new RadixTreePrefix(rdp.ToArray()));
						break;
					}
				}

				return true;
			}
		}
	}
}
