using System;

using Barbados.StorageEngine.Documents.Binary.ValueBuffers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal sealed class ValueName
	{
		private readonly string _name;

		private byte[]? _bytes;

		public ValueName(string name)
		{
			_name = name;
		}

		public ValueStringBuffer GetBuffer()
		{
			return new ValueStringBuffer(_name);
		}

		public ReadOnlySpan<byte> AsSpan()
		{
			if (_bytes is null)
			{
				_bytes = new byte[ValueBufferRawHelpers.GetLength(_name)];
				ValueBufferRawHelpers.WriteStringValue(_bytes, _name);
			}

			return _bytes;
		}
	}
}
