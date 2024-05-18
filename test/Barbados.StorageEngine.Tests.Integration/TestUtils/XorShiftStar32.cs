namespace Barbados.StorageEngine.Tests.Integration.TestUtils
{
	internal sealed class XorShiftStar32(int seed)
	{
		/* RNG implemented manually so that the generated test data stays the same
		 */

		private long _seed = seed + 1;

		public int Next()
		{
			_seed ^= _seed >> 12;
			_seed ^= _seed << 25;
			_seed ^= _seed >> 27;
			return (int)(_seed * 0x2545F4914F6CDD1D >> 32);
		}
	}
}
