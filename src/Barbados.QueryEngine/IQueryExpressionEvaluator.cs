﻿using Barbados.Documents;

namespace Barbados.QueryEngine
{
	internal interface IQueryExpressionEvaluator
	{
		IQueryExpression Expression { get; }

		BarbadosDocument Evaluate(BarbadosDocument document);
	}
}
