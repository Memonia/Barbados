using System;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public ref struct KeyEnumerator
		{
			private readonly bool _flat;
			private RadixTreeBuffer.FullPrefixHasValueEnumerator _enum;

			private RadixTreePrefix _current;
			// Initially contains an invalid prefix
			private RadixTreePrefixSpan _previousTrimmedPrefix;

			public KeyEnumerator(BarbadosDocument document, bool flat)
			{
				_flat = flat;
				_enum = new RadixTreeBuffer.FullPrefixHasValueEnumerator(document._buffer);
				_current = new(Array.Empty<byte>());
				_previousTrimmedPrefix = BarbadosKey.NestingSeparatorAsPrefix;
			}

			public KeyEnumerator(BarbadosDocument document, bool flat, BarbadosKey documentKey)
			{
				_flat = flat;
				_enum = new RadixTreeBuffer.FullPrefixHasValueEnumerator(document._buffer.AsSpan(), documentKey.DocumentSearchPrefix);
				_current = new(Array.Empty<byte>());
				_previousTrimmedPrefix = BarbadosKey.NestingSeparatorAsPrefix;
			}

			public readonly BarbadosKey GetCurrent()
			{
				return new(_current);
			}

			public bool MoveNext()
			{
				while (true)
				{
					if (!_enum.MoveNext())
					{
						return false;
					}

					_current = _enum.GetCurrentPrefix();
					if (_flat)
					{
						break;
					}

					// We know that prefix enumerators will group prefixes with the same root
					// together, because internally they enumerate nodes in depth-first order.
					// We can take advantage of that and skip duplicates by remembering the
					// previous prefix root.
					//
					// In terms of document keys and values, sequences would look something like this:
					//
					// doc1.field11
					// doc1.field22
					// doc1.doc22.field111
					// doc1.doc22.field222
					// doc1.doc22.field333
					// doc2.field11
					// doc2.field22
					// field1
					// field2
					//
					// Thus, we only need to remember the previous trimmed prefix in order to avoid duplicate top level keys

					var trimmed = BarbadosKey.GetFirstNestingLevel(_current);
					if (!_previousTrimmedPrefix.SequenceEqual(trimmed))
					{
						_current = new(trimmed);
						_previousTrimmedPrefix = trimmed;
						break;
					}
				}

				return true;
			}
		}
	}
}
