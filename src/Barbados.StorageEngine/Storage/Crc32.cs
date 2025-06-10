using System;
using System.Numerics;

namespace Barbados.StorageEngine.Storage
{
	internal static class Crc32
	{
		public static uint Combine(uint crc, byte data)
		{
			return BitOperations.Crc32C(crc, data);
		}

		public static uint Combine(uint crc, uint data)
		{
			return BitOperations.Crc32C(crc, data);
		}

		public static uint Combine(uint crc, ulong data)
		{
			return BitOperations.Crc32C(crc, data);
		}

		public static uint Calculate(ReadOnlySpan<byte> data)
		{
			var crc = uint.MaxValue;
			var count = data.Length / sizeof(ulong);
			for (var i = 0; i < count; ++i)
			{
				crc = Combine(crc, HelpRead.AsUInt64(data[(i * sizeof(ulong))..]));
			}

			var remaining = data.Length % sizeof(ulong);
			if (remaining > 0)
			{
				Span<byte> padded = stackalloc byte[sizeof(ulong)];
				data[^remaining..].CopyTo(padded);
				crc = Combine(crc, HelpRead.AsUInt64(padded));
			}

			return crc;
		}
	}
}
