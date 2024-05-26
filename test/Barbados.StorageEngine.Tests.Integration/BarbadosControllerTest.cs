﻿using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed class BarbadosControllerTest
	{
		public sealed class CreateCollection
		{
			[Fact]
			public void CollectionExists_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<CreateCollection>();
				var collection = "test-collection";

				context.Context.Controller.CreateCollection(collection);

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.CreateCollection(collection)
				);
			}

			[Fact]
			public void CollectionDoesNotExist_CreatesCollection()
			{
				using var context = new SelfCleanupBarbadosContext<CreateCollection>();
				var collection = "test-collection";

				context.Context.Controller.CreateCollection(collection);

				Assert.True(context.Context.CollectionExists(collection));
			}
		}

		public sealed class RenameCollection
		{
			[Fact]
			public void CollectionDoesNotExist_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<RenameCollection>();
				var collection = "test-collection";
				var replacement = "new-collection";

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.RenameCollection(collection, replacement)
				);
			}

			[Fact]
			public void CollectionExistsNewNameOccupied_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<RenameCollection>();
				var collection = "test-collection";
				var replacement = "new-collection";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.CreateCollection(replacement);

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.RenameCollection(collection, replacement)
				);

				Assert.True(context.Context.CollectionExists(collection));
				Assert.True(context.Context.CollectionExists(replacement));
			}

			[Fact]
			public void CollectionExistsNewNameNotOccupied_RenamesCollection()
			{
				using var context = new SelfCleanupBarbadosContext<RenameCollection>();
				var collection = "test-collection";
				var replacement = "new-collection";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.RenameCollection(collection, replacement);

				Assert.False(context.Context.CollectionExists(collection));
				Assert.True(context.Context.CollectionExists(replacement));
			}
		}
		
		public sealed class RemoveCollection
		{
			[Fact]
			public void CollectionDoesNotExist_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<RemoveCollection>();
				var collection = "test-collection";

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.RemoveCollection(collection)
				);
			}

			[Fact]
			public void CollectionExists_RemovesCollection()
			{
				using var context = new SelfCleanupBarbadosContext<RemoveCollection>();
				var collection = "test-collection";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.RemoveCollection(collection);

				Assert.False(context.Context.CollectionExists(collection));
			}
		}
		
		public sealed class CreateIndex
		{
			[Fact]
			public void CollectionDoesNotExist_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<CreateIndex>();
				var collection = "test-collection";
				var field = "test-field";

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.CreateIndex(collection, field)
				);
			}

			[Fact]
			public void CollectionExistsIndexExists_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<CreateIndex>();
				var collection = "test-collection";
				var field = "test-field";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.CreateIndex(collection, field);

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.CreateIndex(collection, field)
				);
			}

			[Fact]
			public void CollectionExistsIndexDoesNotExist_CreatesIndex()
			{
				using var context = new SelfCleanupBarbadosContext<CreateIndex>();
				var collection = "test-collection";
				var field = "test-field";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.CreateIndex(collection, field);

				var r = context.Context.IndexExists(collection, field);
				Assert.True(r);
			}
		}

		public sealed class RemoveIndex
		{
			[Fact]
			public void CollectionDoesNotExist_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<RemoveIndex>();
				var collection = "test-collection";
				var field = "test-field";

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.RemoveIndex(collection, field)
				);
			}

			[Fact]
			public void CollectionExistsIndexDoesNotExist_ThrowsException()
			{
				using var context = new SelfCleanupBarbadosContext<RemoveIndex>();
				var collection = "test-collection";
				var field = "test-field";

				context.Context.Controller.CreateCollection(collection);

				Assert.Throws<BarbadosException>(
					() => context.Context.Controller.RemoveIndex(collection, field)
				);
			}

			[Fact]
			public void CollectionExistsIndexExists_RemovesIndex()
			{
				using var context = new SelfCleanupBarbadosContext<RemoveIndex>();
				var collection = "test-collection";
				var field = "test-field";

				context.Context.Controller.CreateCollection(collection);
				context.Context.Controller.CreateIndex(collection, field);
				context.Context.Controller.RemoveIndex(collection, field);

				var r = context.Context.IndexExists(collection, field);
				Assert.False(r);
			}
		}
	}
}
