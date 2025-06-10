using System.Diagnostics;

namespace Barbados.CommonUtils.BitManipulation
{
	public static class BitExtensions
	{
		public static void Set(this ref byte bits, byte mask, bool value) => bits = (byte)(value ? bits | mask : bits & ~mask);
		public static void Set(this ref uint bits, uint mask, bool value) => bits = value ? bits | mask : bits & ~mask;
		public static void Set(this ref ulong bits, ulong mask, bool value) => bits = value ? bits | mask : bits & ~mask;

		public static bool Get(this byte bits, byte mask) => ((ulong)bits).Get(mask);
		public static bool Get(this uint bits, uint mask) => ((ulong)bits).Get(mask);
		public static bool Get(this ulong bits, ulong mask) => (bits & mask) != 0;

		public static void SetBits(this ref byte bits, byte value, byte mask, int shift)
		{
			Debug.Assert(value <= mask >> shift, "Value outside of range");
			bits = (byte)(bits & ~mask | value << shift & mask);
		}

		public static void SetBits(this ref uint bits, uint value, uint mask, int shift)
		{
			Debug.Assert(value <= mask >> shift, "Value outside of range");
			bits = bits & ~mask | value << shift & mask;
		}

		public static void SetBits(this ref ulong bits, ulong value, ulong mask, int shift)
		{
			Debug.Assert(value <= mask >> shift, "Value outside of range");
			bits = bits & ~mask | value << shift & mask;
		}

		public static byte GetBits(this byte bits, byte mask, int shift) => (byte)((ulong)bits).GetBits(mask, shift);
		public static uint GetBits(this uint bits, uint mask, int shift) => (uint)((ulong)bits).GetBits(mask, shift);
		public static ulong GetBits(this ulong bits, ulong mask, int shift) => (bits & mask) >> shift;
	}
}
