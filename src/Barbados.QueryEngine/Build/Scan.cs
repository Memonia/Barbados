using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build
{
	internal sealed class Scan(ValueSelector selector) : IQueryPlan
	{
		public IQueryPlan? Child { get; } = null;
		public ValueSelector Selector { get; } = selector;

		public override string ToString() => FormatHelpers.FormatValueSelector("Scan", Selector);
	}
}