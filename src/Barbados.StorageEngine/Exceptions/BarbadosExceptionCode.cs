namespace Barbados.StorageEngine.Exceptions
{
	public enum BarbadosExceptionCode
	{
		DatabaseDoesNotExist = 1,
		DatabaseVersionMismatch,
		UnexpectedEndOfFile,
		CollectionDoesNotExist,
		CollectionAlreadyExists,
		LockableDoesNotExist,
		StaleLockable,
		CursorClosed,
		CursorConsumed,
		IndexAbandoned,
		IndexDoesNotExist,
		IndexAlreadyExists,
		DocumentNotFound,
		InvalidDocument,
		InvalidOperation,
		MaxPageCountReached,
		MaxFileLengthReached,
		MaxDocumentCountReached,
		InternalError,
	}
}
