using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal sealed partial class ObjectBuffer
	{
		/* The object table and the name table are limited to 16MB (2^24 bytes) in length
		 */

		public const int MaxNameTableLength = 1 << 24;
		public const int MaxValueTableLength = 1 << 24;

		/* Buffer structure:
		 * 	<header>:
		 * 	  I32 descriptor table length
		 *	  I32 value table length
		 *	  I32 name table length
		 *	
		 *	<value descriptor table>:
		 *	  ValueDescriptor1, ValueDescriptor2, ...
		 *	
		 *	<value table>:
		 *	  obj1, obj2, ...
		 *	
		 *	<name table>:
		 *	  string1, string2, ... 
		 *	  
		 *	Names are sorted by the ValueTypeMarker.String comparer. 
		 *	Entries in other tables follow the sort order of the names
		 */

		public int Length => _buffer.Length;

		private readonly byte[] _buffer;

		public ObjectBuffer(byte[] buffer)
		{
			_buffer = buffer;
		}

		public int Count()
		{
			return _count(_buffer);
		}

		public NameEnumerator GetNameEnumerator()
		{
			return new(this);
		}
		
		public bool PrefixExists(ReadOnlySpan<byte> name)
		{
			return _descriptorBinarySearch(_buffer, name, prefixComparison: true) >= 0;
		}

		public bool ValueExists(ReadOnlySpan<byte> name)
		{
			return ValueExists(name, out _, out _);
		}

		public bool ValueExists(ReadOnlySpan<byte> name, out ValueTypeMarker marker)
		{
			return ValueExists(name, out marker, out _);
		}

		public bool ValueExists(ReadOnlySpan<byte> name, out ValueTypeMarker marker, out bool isArray)
		{
			var index = _descriptorBinarySearch(_buffer, name, prefixComparison: false);
			if (index < 0)
			{
				marker = default!;
				isArray = default!;
				return false;
			}

			var descriptor = _getDescriptor(_buffer, index);
			marker = descriptor.Marker;
			isArray = descriptor.IsArray;
			return true;
		}

		public bool TryCollect(ReadOnlySpan<byte> group, bool truncateNames, out ObjectBuffer buffer)
		{
			buffer = Collect(_buffer, group);
			if (buffer.Count() < 1)
			{
				return false;
			}

			if (truncateNames)
			{
				var acc = new Builder.Accumulator();
				var e = buffer.GetNameEnumerator();
				while (e.TryGetNext(out var raw, out _))
				{
					var r = buffer.TryGetBuffer(raw, out var valueBuffer);
					Debug.Assert(r);

					Debug.Assert(raw.Length > group.Length);
					var truncated = raw[group.Length..];

					acc.Add(
						new(ValueBufferRawHelpers.ReadStringFromValue(truncated)), valueBuffer
					);
				}

				buffer = Builder.Build(acc);
				Debug.Assert(buffer.Count() > 0);
			}

			return true;
		}

		public bool TryGetBuffer(ReadOnlySpan<byte> name, out IValueBuffer buffer)
		{
			if (_tryGetBufferRaw(name, out var span, out var descriptor))
			{
				buffer = descriptor.IsArray
					? ValueBufferFactory.CreateFromRawBufferArray(span, descriptor.Marker)
					: ValueBufferFactory.CreateFromRawBuffer(span, descriptor.Marker);

				return true;
			}

			buffer = default!;
			return false;
		}

		public bool TryGetBufferRaw(ReadOnlySpan<byte> name, ValueTypeMarker marker, out ReadOnlySpan<byte> buffer)
		{
			if (_tryGetBufferRaw(name, out var span, out var descriptor) && descriptor.Marker == marker && !descriptor.IsArray)
			{
				buffer = span;
				return true;
			}

			buffer = default!;
			return false;
		}

		public bool TryGetBufferArrayRaw(ReadOnlySpan<byte> name, ValueTypeMarker marker, out ReadOnlySpan<byte> buffer)
		{
			if (_tryGetBufferRaw(name, out var span, out var descriptor) && descriptor.Marker == marker && descriptor.IsArray)
			{
				buffer = span;
				return true;
			}

			buffer = default!;
			return false;
		}

		public bool TryGetBufferValueBytesRaw(ReadOnlySpan<byte> name, out ValueTypeMarker marker, out ReadOnlySpan<byte> value)
		{
			if (_tryGetBufferRaw(name, out var span, out var descriptor) && !descriptor.IsArray)
			{
				marker = descriptor.Marker;
				value = ValueBufferRawHelpers.GetBufferValueBytes(span, descriptor.Marker);
				return true;
			}

			marker = default!;
			value = default!;
			return false;
		}

		public bool TryGetNormalisedValue(ReadOnlySpan<byte> name, out NormalisedValue value)
		{
			if (_tryGetBufferRaw(name, out var span, out var descriptor) && !descriptor.IsArray)
			{
				value = descriptor.Marker switch
				{

					ValueTypeMarker.Int8 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt8(span)),
					ValueTypeMarker.Int16 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt16(span)),
					ValueTypeMarker.Int32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt32(span)),
					ValueTypeMarker.Int64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt64(span)),
					ValueTypeMarker.UInt8 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt8(span)),
					ValueTypeMarker.UInt16 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt16(span)),
					ValueTypeMarker.UInt32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt32(span)),
					ValueTypeMarker.UInt64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt64(span)),
					ValueTypeMarker.Float32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadFloat32(span)),
					ValueTypeMarker.Float64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadFloat64(span)),
					ValueTypeMarker.DateTime => NormalisedValue.Create(ValueBufferRawHelpers.ReadDateTime(span)),
					ValueTypeMarker.Boolean => NormalisedValue.Create(ValueBufferRawHelpers.ReadBoolean(span)),
					ValueTypeMarker.String => NormalisedValue.Create(ValueBufferRawHelpers.ReadStringFromBuffer(span)),
					_ => throw new NotImplementedException()
				};

				return true;
			}

			value = default!;
			return false;
		}

		private bool _tryGetBufferRaw(ReadOnlySpan<byte> name, out ReadOnlySpan<byte> buffer, out ValueDescriptor descriptor)
		{
			var index = _descriptorBinarySearch(_buffer, name, prefixComparison: false);
			if (index >= 0)
			{
				descriptor = _getDescriptor(_buffer, index);
				buffer = _getValueBufferBytes(_buffer, descriptor);
				return true;
			}

			buffer = default!;
			descriptor = default!;
			return false;
		}

		public ReadOnlySpan<byte> AsReadonlySpan() => _buffer;
	}
}
