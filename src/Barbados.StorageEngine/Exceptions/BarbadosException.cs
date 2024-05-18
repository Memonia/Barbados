using System;

namespace Barbados.StorageEngine.Exceptions
{
	public class BarbadosException : Exception
	{
		public BarbadosExceptionCode Code { get; }

		public BarbadosException(BarbadosExceptionCode code) : base($"BE{(int)code:X8}")
		{

		}

		public BarbadosException(BarbadosExceptionCode code, string message) : base($"{message} (BE{(int)code:X8})")
		{

		}
	}
}
