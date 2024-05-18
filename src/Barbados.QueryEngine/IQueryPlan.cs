namespace Barbados.QueryEngine
{
	internal interface IQueryPlan
	{
		IQueryPlan? Child { get; }
	}
}
