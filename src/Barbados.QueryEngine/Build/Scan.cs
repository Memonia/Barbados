using Barbados.Documents;
using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Build
{
	internal sealed class Scan(BarbadosKeySelector selector) : IQueryPlan
	{
		public IQueryPlan? Child { get; } = null;
		public BarbadosKeySelector Selector { get; } = selector;

		public override string ToString() => FormatHelpers.FormatValueSelector("Scan", Selector);
	}
}
