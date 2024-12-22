using System;
using System.Collections;
using System.Collections.Generic;

namespace Barbados.Documents
{
	public sealed class BarbadosKeySelector : IEnumerable<BarbadosKey>
	{
		public static BarbadosKeySelector SelectAll { get; } = new() { All = true };
		public static BarbadosKeySelector SelectNone { get; } = new() { All = false };

		private readonly BarbadosKey[] _keys;

		public bool All { get; private init; }
		public int Count => _keys.Length;

		public BarbadosKeySelector(IEnumerable<BarbadosKey> keys) : this([.. keys])
		{

		}

		public BarbadosKeySelector(params BarbadosKey[] keys)
		{
			static void _throwContains(HashSet<BarbadosKey> keys, BarbadosKey check, BarbadosKey original)
			{
				if (keys.TryGetValue(check, out var stored))
				{
					throw new ArgumentException(
						$"Conflicting keys '{original}' and '{stored}'.", nameof(keys)
					);
				}
			}

			All = false;
			_keys = keys;

			var valueKeys = new HashSet<BarbadosKey>();
			foreach (var key in _keys)
			{
				// Check for duplicates
				if (valueKeys.Contains(key))
				{
					throw new ArgumentException("Duplicate keys", nameof(keys));
				}

				// Check for a case when a document is selected together with one of its members
				if (key.IsDocument)
				{
					var valueKey = key.GetValueKey();
					_throwContains(valueKeys, valueKey, key);
				}

				else
				{
					var docKey = key.GetDocumentKey();
					_throwContains(valueKeys, docKey, key);
				}

				valueKeys.Add(key);
			}
		}

		public IEnumerator<BarbadosKey> GetEnumerator() => ((IEnumerable<BarbadosKey>)_keys).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public BarbadosKey this[int index]
		{
			get => _keys[index];
			set => _keys[index] = value;
		}
	}
}
