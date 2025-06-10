using System;
using System.Diagnostics;

using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		public sealed partial class Enumerator
		{
			private readonly BTreeContext _context;
			private readonly BTreeFindOptions _options;
			private long _skipCount;
			private long _takeCount;

			// Hopefully there won't be too many remainders, so it won't cause OutOfMemoryException.
			// TODO: we can support the case when there are way too many remainders or the remainders
			// are large by storing the total length of all remainders in overflow info and then make
			// a decision (based on a configurable threshold) whether we can keep them all in memory
			// or create some temporary on-disk structure for enumeration purposes
			private readonly RemainderList _remainders;

			private BTreeLeafPage? _currentPage;
			private int _index;

			public Enumerator(BTreeContext context, BTreeFindOptions options)
			{
				_context = context;
				_options = options;
				_skipCount = _options.Skip ?? 0;
				_takeCount = _options.Limit ?? long.MaxValue;
				_index = 0;
				_remainders = new(enumerateForwards: !_options.Reverse);
			}

			public bool MoveNext()
			{
				if (_index < 0)
				{
					return false;
				}

				if (_takeCount <= 0)
				{
					_endEnumeration();
					return false;
				}

				if (_currentPage is null)
				{
					_init();
					if (_index >= 0)
					{
						if (_skipCount > 0)
						{
							_skipCount -= 1;
							return MoveNext();
						}

						_takeCount -= 1;
						return true;
					}

					_endEnumeration();
					return false;
				}

				while (true)
				{
					bool match;
					if (_remainders.Count > 0)
					{
						_remainders.RemoveCurrent();
						if (_remainders.Count == 0)
						{
							continue;
						}

						match = true;
					}

					else
					{
						_moveIndex();
						if (!_currentPage.TryGetKey(_index, out var currentKey))
						{
							if (_options.Reverse)
							{
								if (_currentPage.Previous.IsNull)
								{
									_endEnumeration();
									return false;
								}

								_currentPage = _context.Transaction.Load<BTreeLeafPage>(_currentPage.Previous);
							}

							else
							{
								if (_currentPage.Next.IsNull)
								{
									_endEnumeration();
									return false;
								}

								_currentPage = _context.Transaction.Load<BTreeLeafPage>(_currentPage.Next);
							}

							_initIndexForCurrentPage(setBeforeFirstEntry: false);
							var r = _currentPage.TryGetKey(_index, out currentKey);
							Debug.Assert(r);
						}

						if (_currentPage.TryReadOverflowInfo(currentKey, out _))
						{
							match = _initOverflowEnumerationForCurrentIndex() && _remainders.Count > 0;
						}

						else
						{
							match = _options.Check.Check(currentKey);
						}
					}

					if (!match)
					{
						_endEnumeration();
						return false;
					}

					if (_skipCount > 0)
					{
						_skipCount -= 1;
						continue;
					}

					break;
				}

				_takeCount -= 1;
				return true;
			}

			public bool TryGetCurrentKey(out BTreeNormalisedValue key)
			{
				if (_currentPage is null || !_currentPage.TryGetKey(_index, out var lk) || _remainders.Count == 0)
				{
					key = default!;
					return false;
				}

				var remainder = _remainders.GetCurrent(out _);
				var kbuf = new byte[lk.Separator.Bytes.Length + remainder.Length];
				var kbufs = kbuf.AsSpan();
				var i = 0;
				lk.Separator.Bytes.CopyTo(kbufs[i..]);
				i += lk.Separator.Bytes.Length;
				remainder.CopyTo(kbufs[i..]);

				key = new BTreeNormalisedValue(kbuf);
				return true;
			}

			public bool TryGetCurrentKeyAsSpan(out BTreeNormalisedValueSpan key)
			{
				if (_currentPage is null || !_currentPage.TryGetKey(_index, out var lk) || _remainders.Count > 0)
				{
					key = default!;
					return false;
				}

				key = lk.Separator;
				return true;
			}

			public bool TryGetCurrentData(out byte[] data)
			{
				if (_currentPage is null || !_currentPage.TryGetKey(_index, out var key) || _remainders.Count == 0)
				{
					data = default!;
					return false;
				}

				_remainders.GetCurrent(out var seqNumber);

				// TODO: reduce memory allocations.
				// In case data consists of a single chunk, we could return the span and avoid allocation
				return _context._tryReadChunkedData(key, seqNumber, ChunkType.DataChunk, out data);
			}

			public bool TryGetCurrentDataAsSpan(out ReadOnlySpan<byte> data)
			{
				if (_currentPage is null || !_currentPage.TryGetKey(_index, out var key) || _remainders.Count > 0)
				{
					data = default!;
					return false;
				}

				return _currentPage.TryReadData(key, out data);
			}

			private void _endEnumeration()
			{
				_index = -1;
				_currentPage = null;
			}

			private void _moveIndex()
			{
				_index += _options.Reverse ? -1 : 1;
			}

			private void _initIndexForCurrentPage(bool setBeforeFirstEntry)
			{
				Debug.Assert(_currentPage is not null);
				_index = _options.Reverse ? _currentPage!.Count - 1 : 0;
				if (setBeforeFirstEntry)
				{
					_index += _options.Reverse ? 1 : -1;
				}
			}

			private bool _initOverflowEnumerationForCurrentIndex()
			{
				Debug.Assert(_currentPage is not null);
				if (!_currentPage!.TryGetKey(_index, out var k))
				{
					throw new BarbadosInternalErrorException(
						"Could not initialise overflow enumeration. Key at the given index does not exist"
					);
				}

				if (!_currentPage!.TryReadOverflowInfo(k, out var info))
				{
					return false;
				}

				Debug.Assert(_remainders.Count == 0);
				long seqNum = 0;
				while (seqNum < info.NextSequenceNumber)
				{
					// TODO: reduce memory allocations.
					// We could read the remainder chunk-by-chunk and compare on the go
					if (_context._tryReadChunkedData(k, seqNum, ChunkType.KeyChunk, out var remainder))
					{
						if (_options.Check.Check(k, BTreeNormalisedValueSpan.FromNormalised(remainder)))
						{
							_remainders.Add([.. remainder], seqNum);
						}
					}

					seqNum += 1;
				}

				return true;
			}

			private void _init()
			{
				var search = _options.Reverse
					? _context._toLookupKey(_options.Check.Max.AsSpan(), out _)
					: _context._toLookupKey(_options.Check.Min.AsSpan(), out _)
				;

				if (_context._tryFind(search, out var traceback))
				{
					_currentPage = _context.Transaction.Load<BTreeLeafPage>(traceback.Current);
					_initIndexForCurrentPage(setBeforeFirstEntry: false);

					// A page might contain both chunk storage part and an actual tree part (for example, it happens
					// when a tree contains a single overflowed key with some data). In that case we skip all chunks
					// and ensure the first 'MoveNext' call starts on an actual key
					while (_currentPage.TryGetKey(_index, out var key))
					{
						if (!key.Separator.Marker.IsInternalMarker())
						{
							break;
						}

						_moveIndex();
					}

					// After we've skipped chunks, we need to find the first key that matches the range check
					while (_currentPage.TryGetKey(_index, out var key))
					{
						if (_currentPage.TryReadOverflowInfo(key, out _))
						{
							if (_initOverflowEnumerationForCurrentIndex() && _remainders.Count > 0)
							{
								break;
							}
						}

						else
						if (_options.Check.Check(key))
						{
							break;
						}

						_moveIndex();
					}

					if (!_currentPage.TryGetKey(_index, out _))
					{
						// We shouldn't end up here in a normal scenario, because '_tryFind' gets us to
						// the correct page where we *must* find a key matching the criteria. This is here
						// for debugging purposes
						_endEnumeration();
					}
				}

				else
				{
					_endEnumeration();
				}
			}
		}
	}
}
