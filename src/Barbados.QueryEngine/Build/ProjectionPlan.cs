using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Build
{
	internal sealed class ProjectionPlan : IQueryPlan
	{
		public IQueryPlan Child { get; }
		public KeySelection Selection { get; }

		public ProjectionPlan(IQueryPlan input, KeySelection selection)
		{
			Child = input;
			Selection = selection;
		}

		public override string ToString() => FormatHelpers.FormatSelection("Projection", Selection);
	}
}
