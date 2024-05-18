using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public ref struct NameEnumerator(ObjectBuffer obj)
		{
			private readonly ObjectBuffer _obj = obj;
			private readonly int _count = obj.Count();
			private int _currentIndex = -1;

			public bool TryGetNext(out ReadOnlySpan<byte> raw)
			{
				if (_tryGetNextBuffer(out var rawBuffer))
				{
					raw = ValueBufferRawHelpers.GetBufferValueBytes(rawBuffer, ValueTypeMarker.String);
					return true;
				}

				raw = default!;
				return false;
			}

			public bool TryGetNext(out ReadOnlySpan<byte> raw, out string name)
			{
				if (_tryGetNextBuffer(out var rawBuffer))
				{
					raw = ValueBufferRawHelpers.GetBufferValueBytes(rawBuffer, ValueTypeMarker.String);
					name = ValueBufferRawHelpers.ReadStringFromBuffer(rawBuffer);
					return true;
				}

				raw = default!;
				name = default!;
				return false;
			}

			private bool _tryGetNextBuffer(out ReadOnlySpan<byte> rawBuffer)
			{
				if (_currentIndex < _count - 1)
				{
					_currentIndex += 1;
					rawBuffer = _getNameBufferBytes(_obj._buffer, _getDescriptor(_obj._buffer, _currentIndex));
					return true;
				}

				rawBuffer = default!;
				return false;
			}
		}
	}
}
