namespace Barbados.StorageEngine.Exceptions
{
	public sealed class BarbadosConcurrencyException : BarbadosException
	{
		public BarbadosConcurrencyException(BarbadosExceptionCode code, string message) : base(code, message)
		{

		}
	}
}
