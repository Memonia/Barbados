using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build.Expressions
{
	internal sealed class ConstantExpression<T>(T value, BarbadosIdentifier name) : IQueryExpression
	{
		public T Value { get; } = value;
		public BarbadosIdentifier Name { get; } = name;

		public override string ToString()
		{
			return $"{Value}";
		}
	}
}
