using System;
using System.Linq;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed class CollectionControllerTest
	{
		public sealed class EnsureCreated : SetupTeardownBarbadosContextTestClass<EnsureCreated>
		{
			[Test]
			public void CollectionDoesNotExist_CollectionCreated()
			{
				var name = nameof(CollectionDoesNotExist_CollectionCreated);
				var before = Context.Database.Collections.Exists(name);
				Context.Database.Collections.EnsureCreated(name);
				var after = Context.Database.Collections.Exists(name);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
				});
			}

			[Test]
			public void CollectionExists_CollectionNotCreated()
			{
				var name = nameof(CollectionExists_CollectionNotCreated);
				Context.Database.Collections.Create(name);
				var before = Context.Database.Collections.List().Count();
				Context.Database.Collections.EnsureCreated(name);
				var after = Context.Database.Collections.List().Count();

				Assert.That(before, Is.EqualTo(after));
			}

			[Test]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.EnsureCreated(BarbadosDbObjects.Collections.MetaCollection)
				);
			}

			[Test]
			public void WithAutomaticIdGenerationMode_CollectionDoesNotExist_CollectionCreatedWithMode()
			{
				var name = nameof(WithAutomaticIdGenerationMode_CollectionDoesNotExist_CollectionCreatedWithMode);
				var opts = new CreateCollectionOptions()
				{
					AutomaticIdGeneratorMode = AutomaticIdGeneratorMode.BetterWritePerformance
				};

				var before = Context.Database.Collections.Exists(name);
				Context.Database.Collections.EnsureCreated(name, opts);
				var after = Context.Database.Collections.Exists(name);
				var collection = Context.Database.Collections.Get(name);
				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
					Assert.That(collection.AutomaticIdGeneratorMode, Is.EqualTo(opts.AutomaticIdGeneratorMode));
				});
			}
		}

		public sealed class EnsureDeleted : SetupTeardownBarbadosContextTestClass<EnsureDeleted>
		{
			[Test]
			public void CollectionExists_CollectionDeleted()
			{
				var name = nameof(CollectionExists_CollectionDeleted);
				Context.Database.Collections.Create(name);
				var before = Context.Database.Collections.Exists(name);
				Context.Database.Collections.EnsureDeleted(name);
				var after = Context.Database.Collections.Exists(name);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void CollectionDoesNotExist_CollectionNotDeleted()
			{
				var name = nameof(CollectionDoesNotExist_CollectionNotDeleted);
				var before = Context.Database.Collections.Exists(name);
				Context.Database.Collections.EnsureDeleted(name);
				var after = Context.Database.Collections.Exists(name);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.EnsureDeleted(BarbadosDbObjects.Collections.MetaCollection)
				);
			}
		}

		public sealed class Exists : SetupTeardownBarbadosContextTestClass<Exists>
		{
			[Test]
			public void ById_CollectionExists_ReturnsTrue()
			{
				var name = nameof(ById_CollectionExists_ReturnsTrue);
				Context.Database.Collections.Create(name);
				var collection = Context.Database.Collections.Get(name);
				var exists = Context.Database.Collections.Exists(collection.Id);

				Assert.That(exists, Is.True);
			}

			[Test]
			public void ById_CollectionDoesNotExist_ReturnsFalse()
			{
				var exists = Context.Database.Collections.Exists(ObjectId.Invalid);

				Assert.That(exists, Is.False);
			}

			[Test]
			public void ByName_CollectionExists_ReturnsTrue()
			{
				var name = nameof(ByName_CollectionExists_ReturnsTrue);
				Context.Database.Collections.Create(name);
				var exists = Context.Database.Collections.Exists(name);

				Assert.That(exists, Is.True);
			}

			[Test]
			public void ByName_CollectionDoesNotExist_ReturnsFalse()
			{
				var name = nameof(ByName_CollectionDoesNotExist_ReturnsFalse);
				var exists = Context.Database.Collections.Exists(name);

				Assert.That(exists, Is.False);
			}
		}

		public sealed class Get : SetupTeardownBarbadosContextTestClass<Get>
		{
			[Test]
			public void ById_CollectionExists_ReturnsCollection()
			{
				var name = nameof(ById_CollectionExists_ReturnsCollection);
				Context.Database.Collections.Create(name);
				var collection = Context.Database.Collections.Get(name);
				var result = Context.Database.Collections.Get(collection.Id);

				Assert.That(collection.Id, Is.EqualTo(result.Id));
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Get(ObjectId.Invalid)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Get(new ObjectId(-1))
				);
			}

			[Test]
			public void ByName_CollectionExists_ReturnsCollection()
			{
				var name = nameof(ByName_CollectionExists_ReturnsCollection);
				Context.Database.Collections.Create(name);
				var result = Context.Database.Collections.Get(name);

				Assert.That(name, Is.EqualTo(result.Name.Name));
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Get(name)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Get(BarbadosDbObjects.Collections.MetaCollection)
				);
			}
		}

		public sealed class Create : SetupTeardownBarbadosContextTestClass<Create>
		{
			[Test]
			public void CollectionDoesNotExist_CollectionCreated()
			{
				var name = nameof(CollectionDoesNotExist_CollectionCreated);
				var before = Context.Database.Collections.List().Count();
				Context.Database.Collections.Create(name);
				var after = Context.Database.Collections.List().Count();
				var collection = Context.Database.Collections.Get(name);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.EqualTo(after - 1));
					Assert.That(name, Is.EqualTo(collection.Name.Name));
				});
			}

			[Test]
			public void CollectionExists_Throws()
			{
				var name = nameof(CollectionExists_Throws);
				Context.Database.Collections.Create(name);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Create(name)
				);
			}

			[Test]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Create(BarbadosDbObjects.Collections.MetaCollection)
				);
			}

			[Test]
			public void WithAutomaticIdGenerationMode_CollectionCreatedWithMode()
			{
				var name = nameof(WithAutomaticIdGenerationMode_CollectionCreatedWithMode);
				var opts = new CreateCollectionOptions()
				{
					AutomaticIdGeneratorMode = AutomaticIdGeneratorMode.BetterWritePerformance
				};

				Context.Database.Collections.Create(name, opts);
				var collection = Context.Database.Collections.Get(name);
				Assert.That(collection.AutomaticIdGeneratorMode, Is.EqualTo(opts.AutomaticIdGeneratorMode));
			}
		}

		public sealed class Rename : SetupTeardownBarbadosContextTestClass<Rename>
		{
			[Test]
			public void ById_CollectionExists_CollectionRenamed()
			{
				var name = nameof(ById_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosDbObjectName(name + "-new");
				Context.Database.Collections.Create(name);
				var collection = Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var countBefore = Context.Database.Collections.List().Count();
				Context.Database.Collections.Rename(collectionId, replacement);
				var countAfter = Context.Database.Collections.List().Count();
				var renamedCollection = Context.Database.Collections.Get(replacement);

				Assert.Multiple(() =>
				{
					Assert.That(countBefore, Is.EqualTo(countAfter));
					Assert.That(collectionId, Is.EqualTo(renamedCollection.Id));
					Assert.That(replacement, Is.EqualTo(renamedCollection.Name));
				});
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				var replacement = new BarbadosDbObjectName(nameof(ById_CollectionDoesNotExist_Throws));
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Rename(ObjectId.Invalid, replacement)
				);
			}

			[Test]
			public void ById_CollectionWithTheSameNameExists_Throws()
			{
				var name = nameof(ById_CollectionWithTheSameNameExists_Throws);
				var replacement = new BarbadosDbObjectName(name);
				Context.Database.Collections.Create(name);
				Context.Database.Collections.Create(name + "-new");
				var collection = Context.Database.Collections.Get(name);
				var collectionId = collection.Id;

				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Rename(collectionId, replacement)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				var replacement = new BarbadosDbObjectName(nameof(ById_ReservedCollectionId_Throws));
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Rename(new ObjectId(-1), replacement)
				);
			}

			[Test]
			public void ByName_CollectionExists_CollectionRenamed()
			{
				var name = nameof(ByName_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosDbObjectName(name + "-new");
				Context.Database.Collections.Create(name);
				var collection = Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var countBefore = Context.Database.Collections.List().Count();
				Context.Database.Collections.Rename(name, replacement);
				var countAfter = Context.Database.Collections.List().Count();
				var renamedCollection = Context.Database.Collections.Get(replacement);

				Assert.Multiple(() =>
				{
					Assert.That(countBefore, Is.EqualTo(countAfter));
					Assert.That(collectionId, Is.EqualTo(renamedCollection.Id));
					Assert.That(replacement, Is.EqualTo(renamedCollection.Name));
				});
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosDbObjectName(name + "-new");
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Rename(name, replacement)
				);
			}

			[Test]
			public void ByName_CollectionWithTheSameNameExists_Throws()
			{
				var name = nameof(ByName_CollectionWithTheSameNameExists_Throws);
				var replacement = new BarbadosDbObjectName(name);
				Context.Database.Collections.Create(name);
				Context.Database.Collections.Create(name + "-new");

				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Rename(name, replacement)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				var replacement = new BarbadosDbObjectName(nameof(ByName_ReservedCollectionName_Throws));
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Rename(BarbadosDbObjects.Collections.MetaCollection, replacement)
				);
			}
		}

		public sealed class Delete : SetupTeardownBarbadosContextTestClass<Delete>
		{
			[Test]
			public void ById_CollectionExists_CollectionDeleted()
			{
				var name = nameof(ById_CollectionExists_CollectionDeleted);
				Context.Database.Collections.Create(name);
				var collection = Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var before = Context.Database.Collections.Exists(collectionId);
				Context.Database.Collections.Delete(collectionId);
				var after = Context.Database.Collections.Exists(collectionId);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Delete(ObjectId.Invalid)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Delete(new ObjectId(-1))
				);
			}

			[Test]
			public void ByName_CollectionExists_CollectionDeleted()
			{
				var name = nameof(ByName_CollectionExists_CollectionDeleted);
				Context.Database.Collections.Create(name);
				var before = Context.Database.Collections.Exists(name);
				Context.Database.Collections.Delete(name);
				var after = Context.Database.Collections.Exists(name);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Collections.Delete(name)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Collections.Delete(BarbadosDbObjects.Collections.MetaCollection)
				);
			}
		}
	}
}
