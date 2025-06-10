using System;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public ref struct KeyStringEnumerator
		{
			public string Current { get; private set; }

			private readonly bool _flat;
			private readonly RadixTreeBuffer.PrefixStringValueEnumerator _enum;
			private ReadOnlySpan<char> _previousKeyRootDocumentPortion;

			public KeyStringEnumerator(BarbadosDocument document, bool flat)
			{
				Current = null!;
				_flat = flat;
				_enum = document._buffer.GetPrefixStringValueEnumerator();
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

					// See 'BarbadosDocument.KeyEnumerator'
					var rdp = BarbadosKey.GetRootDocumentPortion(key);
					if (!_previousKeyRootDocumentPortion.StartsWith(rdp))
					{
						_previousKeyRootDocumentPortion = rdp;
						Current = new(rdp);
						break;
					}
				}

				return true;
			}
		}
	}
}
