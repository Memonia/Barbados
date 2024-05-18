using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build
{
	internal sealed class Projection(IQueryPlan input, ValueSelector selector) : IQueryPlan
	{
		public IQueryPlan Child { get; } = input;
		public ValueSelector Selector { get; } = selector;

		public override string ToString() => FormatHelpers.FormatValueSelector("Projection", Selector);
	}
}
