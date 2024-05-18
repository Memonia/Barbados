namespace Barbados.StorageEngine.Exceptions
{
	internal sealed class BarbadosConcurrencyException : BarbadosException
	{
		public BarbadosConcurrencyException(BarbadosExceptionCode code, string message) : base(code, message)
		{

		}
	}
}
