﻿using Barbados.StorageEngine.Collections;

namespace Barbados.StorageEngine
{
	public interface IDatabaseMonitor
	{
		IReadOnlyBarbadosCollection GetInternalCollection(BarbadosDbObjectName collectionName);
	}
}
