using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Barbados.StorageEngine.Caching;
using Barbados.StorageEngine.Paging;

using Microsoft.Win32.SafeHandles;

namespace Barbados.StorageEngine
{
	public sealed class BarbadosContext : IDisposable
	{
		public static void CreateDatabaseFile(string path)
		{
			using var fs = File.Create(path);
			PagePool.AllocateRoot(fs.SafeFileHandle);
		}

		public string DatabaseFilePath { get; }
		public IBarbadosController BarbadosController => Controller;

		internal BarbadosController Controller { get; }

		private bool _disposed;
		private readonly SafeFileHandle _fileHandle;

		public BarbadosContext(string path) : this(path, openOrCreate: false)
		{

		}

		public BarbadosContext(string path, bool openOrCreate) : this(path, openOrCreate, StorageOptions.Default)
		{

		}

		public BarbadosContext(string path, StorageOptions options) : this(path, openOrCreate: false, options)
		{

		}

		public BarbadosContext(string path, bool openOrCreate, StorageOptions options)
		{
			if (openOrCreate && !File.Exists(path))
			{
				CreateDatabaseFile(path);
			}

			DatabaseFilePath = Path.GetFullPath(path);
			var cacheFactory = new CacheFactory(
				options.CachedPageCountLimit,
				options.CachingStrategy
			);

			_fileHandle = File.OpenHandle(DatabaseFilePath, FileMode.Open, FileAccess.ReadWrite);
			Controller = new BarbadosController(
				new PagePool(_fileHandle, cacheFactory),
				new LockManager()
			);

			AppDomain.CurrentDomain.ProcessExit += (sender, e) => Dispose();
		}

		public bool IndexExists(string collection, string field)
		{
			return GetIndexedFields(collection).Any(e => e == field);
		}

		public bool CollectionExists(string name)
		{
			if (name == BarbadosIdentifiers.Collection.MetaCollection)
			{
				return true;
			}

			var meta = Controller.GetMetaCollection();
			return meta.Find(name, out _);
		}

		public IEnumerable<string> GetCollections()
		{
			var meta = Controller.GetMetaCollection();
			yield return meta.Name;

			var cursor = meta.GetCursor();
			foreach (var document in cursor)
			{
				var r = document.TryGetString(
					BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute, out var name
				);

				Debug.Assert(r);
				yield return name;
			}
		}

		public IEnumerable<string> GetIndexedFields(string collection)
		{
			if (collection == BarbadosIdentifiers.Collection.MetaCollection)
			{
				yield return BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute;
				yield break;
			}

			var meta = Controller.GetMetaCollection();
			if (!meta.Find(collection, out var document))
			{
				yield break;
			}

			if (document.TryGetDocumentArray(BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexArray))
			{
				foreach (var index in indexArray)
				{
					var r = index.TryGetString(
						BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, out var name
					);
					
					Debug.Assert(r);
					yield return name;
				}
			}
		}

		public void Dispose()
		{
			_dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void _dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Controller.Pool.Flush();
					_fileHandle.Close();
					_fileHandle.Dispose();
				}

				_disposed = true;
			}
		}
	}
}
