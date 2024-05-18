using System;
using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.Documents.Binary.ValueBuffers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public partial class Builder
		{
			public sealed class Accumulator
			{
				private sealed class NameBuffer : IComparable<NameBuffer>
				{
					private static readonly IValueSpanComparer _comparer =
						ValueSpanComparerFactory.GetComparer(ValueTypeMarker.String);

					public ValueStringBuffer Buffer { get; }

					private readonly byte[] _bytes;

					public NameBuffer(ValueStringBuffer buffer)
					{
						Buffer = buffer;
						_bytes = new byte[buffer.ValueLength];
						Buffer.WriteValueTo(_bytes);
					}

					public int CompareTo(NameBuffer? other)
					{
						return _comparer.Compare(_bytes, other!._bytes);
					}
				}

				public int Count => _buffers.Count;
				public int NameTableLength { get; private set; } = 0;
				public int ValueTableLength { get; private set; } = 0;
				public int DescriptorTableLength { get; private set; } = 0;

				private readonly SortedList<NameBuffer, IValueBuffer> _buffers;

				public Accumulator()
				{
					_buffers = new();
				}

				public bool Contains(ValueStringBuffer nameBuffer)
				{
					return _buffers.ContainsKey(new(nameBuffer));
				}

				public bool TryGet(ValueStringBuffer nameBuffer, out IValueBuffer buffer)
				{
					return _buffers.TryGetValue(new(nameBuffer), out buffer!);
				}

				public void Add(ValueStringBuffer name, IValueBuffer buffer)
				{
					if (
						name.GetLength() > MaxNameTableLength ||
						buffer.GetLength() > MaxValueTableLength ||
						name.GetLength() + NameTableLength > MaxNameTableLength ||
						buffer.GetLength() + ValueTableLength > MaxValueTableLength
					)
					{
						throw new InvalidOperationException("The buffer exceeded the maximum allowed length");
					}

					NameTableLength += name.GetLength();
					ValueTableLength += buffer.GetLength();
					DescriptorTableLength += ValueDescriptor.BinaryLength;
					_buffers.Add(new(name), buffer);
				}

				public void Clear()
				{
					NameTableLength = 0;
					ValueTableLength = 0;
					DescriptorTableLength = 0;
					_buffers.Clear();
				}

				public IEnumerable<ValueStringBuffer> GetSortedNameBuffersEnumerator() => _buffers.Keys.Select(e => e.Buffer);
			}
		}
	}
}
