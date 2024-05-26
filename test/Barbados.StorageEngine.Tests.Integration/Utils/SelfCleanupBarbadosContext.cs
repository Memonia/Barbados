using System.Runtime.CompilerServices;

namespace Barbados.StorageEngine.Tests.Integration.Utils
{
	internal sealed class SelfCleanupBarbadosContext<TTestClass> : IDisposable
	{
		public BarbadosContext Context { get; }

		public SelfCleanupBarbadosContext([CallerMemberName] string caller = "")
		{
			var path = $"{typeof(TTestClass).FullName}-{caller}.test-db";
			File.Delete(path);

			Context = new BarbadosContext(
				path,
				true,
				StorageOptions.Default with { CachedPageCountLimit = 4096 }
			);
		}

		public void Dispose()
		{
			var path = Context.DatabaseFilePath;
			Context.Dispose();
			File.Delete(path);
		}
	}
}
