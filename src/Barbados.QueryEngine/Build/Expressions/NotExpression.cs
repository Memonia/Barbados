namespace Barbados.QueryEngine.Build.Expressions
{
	internal sealed class NotExpression(IQueryExpression input) : IQueryExpression
	{
		public IQueryExpression Expression { get; } = input;

		public override string ToString() => $"(NOT {Expression})";
	}
}
