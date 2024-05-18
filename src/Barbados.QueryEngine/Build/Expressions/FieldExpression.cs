using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build.Expressions
{
	internal sealed class FieldExpression(BarbadosIdentifier name) : IQueryExpression
	{
		public BarbadosIdentifier Name { get; } = name;

		public override string ToString()
		{
			return $"'{Name}'";
		}
	}
}
