using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public partial class Builder
		{
			public sealed class Accumulator
			{
				public int Count => _nameBuffers.Count;
				public int NameTableLength { get; private set; }
				public int ValueTableLength { get; private set; }
				public int DescriptorTableLength { get; private set; }

				private readonly List<(ValueName name, IValueBuffer buffer)> _nameBuffers;

				public Accumulator()
				{
					NameTableLength = 0;
					ValueTableLength = 0;
					DescriptorTableLength = 0;
					_nameBuffers = [];
				}

				public bool Contains(ValueName name)
				{
					var index = _valueNameBinarySearch(name);
					return index >= 0;
				}

				public bool TryGet(ValueName name, out IValueBuffer buffer)
				{
					var index = _valueNameBinarySearch(name);
					if (index >= 0)
					{
						buffer = _nameBuffers[index].buffer;
						return true;
					}

					buffer = default!;
					return false;
				}

				public void Add(ValueName name, IValueBuffer buffer)
				{
					var nameBuffer = name.GetBuffer();
					if (
						nameBuffer.GetLength() > MaxNameTableLength ||
						buffer.GetLength() > MaxValueTableLength ||
						nameBuffer.GetLength() + NameTableLength > MaxNameTableLength ||
						buffer.GetLength() + ValueTableLength > MaxValueTableLength
					)
					{
						throw new InvalidOperationException("The buffer exceeded the maximum allowed length");
					}

					NameTableLength += nameBuffer.GetLength();
					ValueTableLength += buffer.GetLength();
					DescriptorTableLength += ValueDescriptor.BinaryLength;

					var index = _valueNameBinarySearch(name);
					if (index >= 0)
					{
						throw new ArgumentException("Value with a given name already exists", nameof(name));
					}

					_nameBuffers.Insert(~index, (name, buffer));
				}

				public void Clear()
				{
					NameTableLength = 0;
					ValueTableLength = 0;
					DescriptorTableLength = 0;
					_nameBuffers.Clear();
				}

				public IEnumerable<ValueName> GetSortedValueNameEnumerator()
				{
					foreach (var (name, _) in _nameBuffers)
					{
						yield return name;
					}
				}

				private int _valueNameBinarySearch(ValueName name)
				{
					int left = 0;
					int right = _nameBuffers.Count - 1;
					while (left <= right)
					{
						// Idea taken from .NET runtime implementation
						int m = (int)((uint)left + (uint)right >> 1);
						int c = name.AsSpan().SequenceCompareTo(_nameBuffers[m].name.AsSpan());

						if (c == 0)
						{
							return m;
						}

						else
						if (c > 0)
						{
							left = m + 1;
						}

						else
						{
							right = m - 1;
						}
					}

					return ~left;
				}
			}
		}
	}
}
