using System;

namespace Barbados.StorageEngine.Exceptions
{
	public sealed class BarbadosInternalErrorException : BarbadosException
	{
		public BarbadosInternalErrorException() :
			base(BarbadosExceptionCode.InternalError)
		{

		}

		public BarbadosInternalErrorException(string message) :
			base(BarbadosExceptionCode.InternalError, message)
		{

		}

		public BarbadosInternalErrorException(string message, Exception innerException) : 
			base(BarbadosExceptionCode.InternalError, message, innerException)
		{

		}
	}
}
