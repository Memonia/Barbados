using System;
using System.Linq;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class CollectionControllerTest
	{
		public partial class EnsureCreated : IClassFixture<BarbadosContextFixture<EnsureCreated>>
		{
			[Fact]
			public void CollectionDoesNotExist_CollectionCreated()
			{
				var name = nameof(CollectionDoesNotExist_CollectionCreated);
				var before = _fixture.Context.Database.Collections.Exists(name);
				_fixture.Context.Database.Collections.EnsureCreated(name);
				var after = _fixture.Context.Database.Collections.Exists(name);

				Assert.False(before);
				Assert.True(after);
			}

			[Fact]
			public void CollectionExists_CollectionNotCreated()
			{
				var name = nameof(CollectionExists_CollectionNotCreated);
				_fixture.Context.Database.Collections.Create(name);
				var before = _fixture.Context.Database.Collections.List().Count();
				_fixture.Context.Database.Collections.EnsureCreated(name);
				var after = _fixture.Context.Database.Collections.List().Count();

				Assert.Equal(before, after);
			}

			[Fact]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.EnsureCreated(
						CommonIdentifiers.Collections.MetaCollection
					)
				);
			}
		}

		public partial class EnsureDeleted : IClassFixture<BarbadosContextFixture<EnsureDeleted>>
		{
			[Fact]
			public void CollectionExists_CollectionDeleted()
			{
				var name = nameof(CollectionExists_CollectionDeleted);
				_fixture.Context.Database.Collections.Create(name);
				var before = _fixture.Context.Database.Collections.Exists(name);
				_fixture.Context.Database.Collections.EnsureDeleted(name);
				var after = _fixture.Context.Database.Collections.Exists(name);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void CollectionDoesNotExist_CollectionNotDeleted()
			{
				var name = nameof(CollectionDoesNotExist_CollectionNotDeleted);
				var before = _fixture.Context.Database.Collections.Exists(name);
				_fixture.Context.Database.Collections.EnsureDeleted(name);
				var after = _fixture.Context.Database.Collections.Exists(name);

				Assert.False(before);
				Assert.False(after);
			}

			[Fact]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.EnsureDeleted(
						CommonIdentifiers.Collections.MetaCollection
					)
				);
			}
		}

		public sealed partial class Exists : IClassFixture<BarbadosContextFixture<Exists>>
		{
			[Fact]
			public void ById_CollectionExists_ReturnsTrue()
			{
				var name = nameof(ById_CollectionExists_ReturnsTrue);
				_fixture.Context.Database.Collections.Create(name);
				var collection = _fixture.Context.Database.Collections.Get(name);
				var exists = _fixture.Context.Database.Collections.Exists(collection.Id);

				Assert.True(exists);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_ReturnsFalse()
			{
				var exists = _fixture.Context.Database.Collections.Exists(ObjectId.Invalid);

				Assert.False(exists);
			}

			[Fact]
			public void ByName_CollectionExists_ReturnsTrue()
			{
				var name = nameof(ByName_CollectionExists_ReturnsTrue);
				_fixture.Context.Database.Collections.Create(name);
				var exists = _fixture.Context.Database.Collections.Exists(name);

				Assert.True(exists);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_ReturnsFalse()
			{
				var name = nameof(ByName_CollectionDoesNotExist_ReturnsFalse);
				var exists = _fixture.Context.Database.Collections.Exists(name);

				Assert.False(exists);
			}
		}

		public sealed partial class Get : IClassFixture<BarbadosContextFixture<Get>>
		{
			[Fact]
			public void ById_CollectionExists_ReturnsCollection()
			{
				var name = nameof(ById_CollectionExists_ReturnsCollection);
				_fixture.Context.Database.Collections.Create(name);
				var collection = _fixture.Context.Database.Collections.Get(name);
				var result = _fixture.Context.Database.Collections.Get(collection.Id);

				Assert.Equal(collection.Id, result.Id);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Get(ObjectId.Invalid)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Get(new ObjectId(-1))
				);
			}

			[Fact]
			public void ByName_CollectionExists_ReturnsCollection()
			{
				var name = nameof(ByName_CollectionExists_ReturnsCollection);
				_fixture.Context.Database.Collections.Create(name);
				var result = _fixture.Context.Database.Collections.Get(name);

				Assert.Equal(name, result.Name);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Get(name)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Get(CommonIdentifiers.Collections.MetaCollection)
				);
			}
		}

		public sealed partial class Create : IClassFixture<BarbadosContextFixture<Create>>
		{
			[Fact]
			public void CollectionDoesNotExist_CollectionCreated()
			{
				var name = nameof(CollectionDoesNotExist_CollectionCreated);
				var before = _fixture.Context.Database.Collections.List().Count();
				_fixture.Context.Database.Collections.Create(name);
				var after = _fixture.Context.Database.Collections.List().Count();
				var collection = _fixture.Context.Database.Collections.Get(name);

				Assert.Equal(before + 1, after);
				Assert.Equal(name, collection.Name);
			}

			[Fact]
			public void CollectionExists_Throws()
			{
				var name = nameof(CollectionExists_Throws);
				_fixture.Context.Database.Collections.Create(name);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Create(name)
				);
			}

			[Fact]
			public void ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Create(CommonIdentifiers.Collections.MetaCollection)
				);
			}
		}

		public sealed partial class Rename : IClassFixture<BarbadosContextFixture<Rename>>
		{
			[Fact]
			public void ById_CollectionExists_CollectionRenamed()
			{
				var name = nameof(ById_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosIdentifier(name + "-new");
				_fixture.Context.Database.Collections.Create(name);
				var collection = _fixture.Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var countBefore = _fixture.Context.Database.Collections.List().Count();
				_fixture.Context.Database.Collections.Rename(collectionId, replacement);
				var countAfter = _fixture.Context.Database.Collections.List().Count();
				var renamedCollection = _fixture.Context.Database.Collections.Get(replacement);

				Assert.Equal(countBefore, countAfter);
				Assert.Equal(collectionId, renamedCollection.Id);
				Assert.Equal(replacement.Identifier, renamedCollection.Name.Identifier);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				var replacement = new BarbadosIdentifier(nameof(ById_CollectionDoesNotExist_Throws));
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Rename(ObjectId.Invalid, replacement)
				);
			}

			[Fact]
			public void ById_CollectionWithTheSameNameExists_Throws()
			{
				var name = nameof(ById_CollectionWithTheSameNameExists_Throws);
				var replacement = new BarbadosIdentifier(name);
				_fixture.Context.Database.Collections.Create(name);
				_fixture.Context.Database.Collections.Create(name + "-new");
				var collection = _fixture.Context.Database.Collections.Get(name);
				var collectionId = collection.Id;

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Rename(collectionId, replacement)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				var replacement = new BarbadosIdentifier(nameof(ById_ReservedCollectionId_Throws));
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Rename(new ObjectId(-1), replacement)
				);
			}

			[Fact]
			public void ByName_CollectionExists_CollectionRenamed()
			{
				var name = nameof(ByName_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosIdentifier(name + "-new");
				_fixture.Context.Database.Collections.Create(name);
				var collection = _fixture.Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var countBefore = _fixture.Context.Database.Collections.List().Count();
				_fixture.Context.Database.Collections.Rename(name, replacement);
				var countAfter = _fixture.Context.Database.Collections.List().Count();
				var renamedCollection = _fixture.Context.Database.Collections.Get(replacement);

				Assert.Equal(countBefore, countAfter);
				Assert.Equal(collectionId, renamedCollection.Id);
				Assert.Equal(replacement.Identifier, renamedCollection.Name.Identifier);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionExists_CollectionRenamed);
				var replacement = new BarbadosIdentifier(name + "-new");
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Rename(name, replacement)
				);
			}

			[Fact]
			public void ByName_CollectionWithTheSameNameExists_Throws()
			{
				var name = nameof(ByName_CollectionWithTheSameNameExists_Throws);
				var replacement = new BarbadosIdentifier(name);
				_fixture.Context.Database.Collections.Create(name);
				_fixture.Context.Database.Collections.Create(name + "-new");

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Rename(name, replacement)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				var replacement = new BarbadosIdentifier(nameof(ByName_ReservedCollectionName_Throws));
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Rename(CommonIdentifiers.Collections.MetaCollection, replacement)
				);
			}
		}

		public sealed partial class Delete : IClassFixture<BarbadosContextFixture<Delete>>
		{
			[Fact]
			public void ById_CollectionExists_CollectionDeleted()
			{
				var name = nameof(ById_CollectionExists_CollectionDeleted);
				_fixture.Context.Database.Collections.Create(name);
				var collection = _fixture.Context.Database.Collections.Get(name);
				var collectionId = collection.Id;
				var before = _fixture.Context.Database.Collections.Exists(collectionId);
				_fixture.Context.Database.Collections.Delete(collectionId);
				var after = _fixture.Context.Database.Collections.Exists(collectionId);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Delete(ObjectId.Invalid)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Delete(new ObjectId(-1))
				);
			}

			[Fact]
			public void ByName_CollectionExists_CollectionDeleted()
			{
				var name = nameof(ByName_CollectionExists_CollectionDeleted);
				_fixture.Context.Database.Collections.Create(name);
				var before = _fixture.Context.Database.Collections.Exists(name);
				_fixture.Context.Database.Collections.Delete(name);
				var after = _fixture.Context.Database.Collections.Exists(name);
			
				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var name = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Collections.Delete(name)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Collections.Delete(CommonIdentifiers.Collections.MetaCollection)
				);
			}
		}
	}
}
