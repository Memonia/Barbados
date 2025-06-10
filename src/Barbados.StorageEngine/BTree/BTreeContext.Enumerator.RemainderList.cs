using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		public partial class Enumerator
		{
			private sealed class RemainderList
			{
				public int Count => _remaindersSorted.Count;

				private readonly bool _enumerateForwards;
				private readonly List<(long seqNumber, byte[] remainder)> _remaindersSorted;
				private readonly Comparer<(long seqNumber, byte[] remainder)> _remainderComparer;

				public RemainderList(bool enumerateForwards)
				{
					_enumerateForwards = enumerateForwards;
					_remaindersSorted = [];
					_remainderComparer = Comparer<(long seqNumber, byte[] remainder)>.Create(
						(x, y) => x.remainder.AsSpan().SequenceCompareTo(y.remainder.AsSpan())
					);
				}

				public void Add(byte[] remainder, long sequenceNumber)
				{
					var pair = (sequenceNumber, remainder);
					var i = _remaindersSorted.BinarySearch(pair, _remainderComparer);
					if (i >= 0)
					{
						throw new ArgumentException("Given remainder already exists", nameof(remainder));
					}

					_remaindersSorted.Insert(~i, pair);
				}

				public void RemoveCurrent()
				{
					if (_enumerateForwards)
					{
						_remaindersSorted.RemoveAt(0);
					}

					else
					{
						_remaindersSorted.RemoveAt(Count - 1);
					}
				}

				public byte[] GetCurrent(out long sequenceNumber)
				{
					if (_enumerateForwards)
					{
						sequenceNumber = _remaindersSorted[0].seqNumber;
						return _remaindersSorted[0].remainder;
					}

					else
					{
						sequenceNumber = _remaindersSorted[Count - 1].seqNumber;
						return _remaindersSorted[Count - 1].remainder;
					}
				}
			}
		}
	}
}
