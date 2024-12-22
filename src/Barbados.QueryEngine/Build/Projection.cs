using Barbados.Documents;
using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Build
{
	internal sealed class Projection(IQueryPlan input, BarbadosKeySelector selector) : IQueryPlan
	{
		public IQueryPlan Child { get; } = input;
		public BarbadosKeySelector Selector { get; } = selector;

		public override string ToString() => FormatHelpers.FormatValueSelector("Projection", Selector);
	}
}
