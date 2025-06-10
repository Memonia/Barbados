namespace Barbados.StorageEngine.BTree
{
	internal sealed record BTreeFindOptions
	{
		public static BTreeFindOptions FindAll { get; } = new BTreeFindOptions(
			BTreeKeyRangeCheck.ExcludeMinExcludeMax(BTreeNormalisedValue.Min, BTreeNormalisedValue.Max)
		)
		{
			Skip = null,
			Limit = null,
			Reverse = false
		};

		public static BTreeFindOptions FindAllIncludeExternal { get; } = new BTreeFindOptions(
			BTreeKeyRangeCheck.IncludeMinIncludeMax(
				BTreeNormalisedValue.Create(BTreeNormalisedValue.Min, true),
				BTreeNormalisedValue.Create(BTreeNormalisedValue.Max, true)
			)
		)
		{
			Skip = null,
			Limit = null,
			Reverse = false
		};

		public static BTreeFindOptions CreateFindSingle(BTreeNormalisedValue value)
		{
			return new BTreeFindOptions(
				BTreeKeyRangeCheck.IncludeMinIncludeMax(value, value))
			{
				Skip = null,
				Limit = 1,
				Reverse = false
			};
		}

		public required long? Skip { get; init; }
		public required long? Limit { get; init; }
		public required bool Reverse { get; init; }
		public BTreeKeyRangeCheck Check { get; }

		public BTreeFindOptions(BTreeKeyRangeCheck check)
		{
			Check = check;
		}

		public BTreeFindOptions(BTreeNormalisedValue? min, BTreeNormalisedValue? max, bool includeMin, bool includeMax)
		{
			min ??= BTreeNormalisedValue.Min;
			max ??= BTreeNormalisedValue.Max;

			Check =
				includeMin
					? includeMax
						? BTreeKeyRangeCheck.IncludeMinIncludeMax(min, max)
						: BTreeKeyRangeCheck.IncludeMinExcludeMax(min, max)
					: includeMax
						? BTreeKeyRangeCheck.ExcludeMinIncludeMax(min, max)
						: BTreeKeyRangeCheck.ExcludeMinExcludeMax(min, max)
			;
		}
	}
}
