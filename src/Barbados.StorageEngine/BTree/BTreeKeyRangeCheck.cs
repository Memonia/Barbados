using System;

namespace Barbados.StorageEngine.BTree
{
	internal sealed class BTreeKeyRangeCheck
	{
		public static BTreeKeyRangeCheck ExcludeMinExcludeMax(BTreeNormalisedValue min, BTreeNormalisedValue max)
			=> new((min, max) => min > 0 && max < 0) { Min = min, Max = max, IncludeMin = false, IncludeMax = false };

		public static BTreeKeyRangeCheck IncludeMinExcludeMax(BTreeNormalisedValue min, BTreeNormalisedValue max)
			=> new((min, max) => min >= 0 && max < 0) { Min = min, Max = max, IncludeMin = true, IncludeMax = false };

		public static BTreeKeyRangeCheck ExcludeMinIncludeMax(BTreeNormalisedValue min, BTreeNormalisedValue max)
			=> new((min, max) => min > 0 && max <= 0) { Min = min, Max = max, IncludeMin = false, IncludeMax = true };

		public static BTreeKeyRangeCheck IncludeMinIncludeMax(BTreeNormalisedValue min, BTreeNormalisedValue max)
			=> new((min, max) => min >= 0 && max <= 0) { Min = min, Max = max, IncludeMin = true, IncludeMax = true };

		public required bool IncludeMin { get; init; }
		public required bool IncludeMax { get; init; }
		public required BTreeNormalisedValue Min { get; init; }
		public required BTreeNormalisedValue Max { get; init; }

		private readonly Func<int, int, bool> _eval;

		private BTreeKeyRangeCheck(Func<int, int, bool> eval)
		{
			_eval = eval;
		}

		public bool Check(BTreeLookupKeySpan lookupKey)
		{
			return Check(lookupKey.Separator);
		}

		public bool Check(BTreeNormalisedValueSpan key)
		{
			return _eval(
				key.Bytes.SequenceCompareTo(Min.AsSpan().Bytes),
				key.Bytes.SequenceCompareTo(Max.AsSpan().Bytes)
			);
		}

		public bool Check(BTreeLookupKeySpan lookupKey, BTreeNormalisedValueSpan remainder)
		{
			var lookupKeyLength = lookupKey.Separator.Bytes.Length;
			var mins = Min.AsSpan();
			var maxs = Max.AsSpan();
			var minLtEqLookupKey = mins.Bytes.Length <= lookupKeyLength;
			var maxLtEqLookupKey = maxs.Bytes.Length < lookupKeyLength;
			var minlhs = minLtEqLookupKey ? mins.Bytes : mins.Bytes[..lookupKeyLength];
			var maxlhs = maxLtEqLookupKey ? maxs.Bytes : maxs.Bytes[..lookupKeyLength];
			var minr = lookupKey.Separator.Bytes.SequenceCompareTo(minlhs);
			var maxr = lookupKey.Separator.Bytes.SequenceCompareTo(maxlhs);
			if (minr > 0 && maxr < 0)
			{
				return _eval(minr, maxr);
			}

			if (minr == 0 && !minLtEqLookupKey)
			{
				minr = remainder.Bytes.SequenceCompareTo(mins.Bytes[lookupKeyLength..]);
			}

			if (maxr == 0 && !maxLtEqLookupKey)
			{
				maxr = remainder.Bytes.SequenceCompareTo(maxs.Bytes[lookupKeyLength..]);
			}

			return _eval(minr, maxr);
		}
	}
}
