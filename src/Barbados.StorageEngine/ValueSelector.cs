using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Barbados.StorageEngine
{
	public class ValueSelector : IEnumerable<BarbadosIdentifier>
	{
		public static ValueSelector SelectAll { get; } = new() { All = true };

		private readonly BarbadosIdentifier[] _identifiers;

		public bool All { get; private init; }
		public int Count => _identifiers.Length;

		public ValueSelector(IEnumerable<BarbadosIdentifier> identifiers) : this(identifiers.ToArray())
		{

		}

		public ValueSelector(params BarbadosIdentifier[] identifiers)
		{
			static void _throwContains(HashSet<string> identifiers, string check, string original)
			{
				if (identifiers.TryGetValue(check, out var stored))
				{
					throw new ArgumentException(
						$"Conflicting identifiers '{original}' and '{stored}'.", nameof(identifiers)
					);
				}
			}

			All = false;
			_identifiers = identifiers;

			var valueIdentifiers = new HashSet<string>();
			foreach (var identifier in _identifiers)
			{
				// Check for duplicates
				if (valueIdentifiers.Contains(identifier))
				{
					throw new ArgumentException("Duplicate identifiers", nameof(identifiers));
				}

				// Check for a case when a group identifier is given together with one of its members
				if (identifier.IsGroup)
				{
					var valueIdentifier = identifier.GetGroupName();
					_throwContains(valueIdentifiers, valueIdentifier, identifier);
				}

				else
				{
					var groupIdentifier = identifier.GetGroupIdentifier();
					_throwContains(valueIdentifiers, groupIdentifier, identifier);
				}

				valueIdentifiers.Add(identifier);
			}
		}

		public IEnumerator<BarbadosIdentifier> GetEnumerator() =>
			 ((IEnumerable<BarbadosIdentifier>)_identifiers).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public BarbadosIdentifier this[int index]
		{
			get => _identifiers[index];
			set => _identifiers[index] = value;
		}
	}
}
