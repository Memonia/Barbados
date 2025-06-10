using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Build
{
	internal sealed class ScanPlan : IQueryPlan
	{
		public IQueryPlan? Child { get; }
		public KeySelection Selection { get; }

		public ScanPlan(KeySelection selection)
		{
			Selection = selection;
		}

		public override string ToString() => FormatHelpers.FormatSelection("Scan", Selection);
	}
}
