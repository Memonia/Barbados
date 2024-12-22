namespace Barbados.QueryEngine.Build.Expressions
{
	internal sealed class ConstantExpression<T>(T value, string name) : IQueryExpression
	{
		public T Value { get; } = value;
		public string Name { get; } = name;

		public override string ToString()
		{
			return $"{Value}";
		}
	}
}
