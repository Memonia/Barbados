namespace Barbados.StorageEngine.Exceptions
{
	internal sealed class BarbadosInternalErrorExceptionHelpers
	{
		public static BarbadosInternalErrorException TransactionForCurrentThreadDoesNotExist()
		{
			return new BarbadosInternalErrorException("Transaction for current thread does not exist");
		}

		public static BarbadosInternalErrorException CouldNotRetrieveDataFromEnumeratorAfterMoveNext()
		{
			return new BarbadosInternalErrorException("No data could be retrieved from a successfully advanced enumerator");
		}
	}
}
