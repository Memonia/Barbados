namespace Barbados.QueryEngine.Build.Expressions
{
	internal sealed class FieldExpression(string name) : IQueryExpression
	{
		public string Name { get; } = name;

		public override string ToString()
		{
			return $"'{Name}'";
		}
	}
}
