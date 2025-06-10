namespace Barbados.StorageEngine.Exceptions
{
	public enum BarbadosExceptionCode
	{
		DatabaseDoesNotExist = 1,
		InvalidDatabaseState,
		DatabaseVersionMismatch,
		WalDoesNotExist,
		WalVersionMismatch,
		DbMagicWalMagicMismatch,
		UnexpectedEndOfFile,
		ChecksumVerificationFailed,
		MaxPageCountReached,
		MaxTransactionCountReached,
		MaxWalCommitNumberReached,
		MaxAutomaticIdCountReached,
		MaxSamePrefixKeyCountReached,
		DocumentNotFound,
		DocumentAlreadyExists,
		CollectionDoesNotExist,
		CollectionAlreadyExists,
		IndexDoesNotExist,
		IndexAlreadyExists,
		LockDoesNotExist,
		CursorClosed,
		CursorConsumed,
		NestedTransactionDetected,
		TransactionDoesNotExist,
		TransactionUpgradeAttempt,
		TransactionScopeMismatch,
		TransactionScopeCompleted,
		TransactionTargetMismatch,
		InternalError = 0x7FFFFFFF
	}
}
