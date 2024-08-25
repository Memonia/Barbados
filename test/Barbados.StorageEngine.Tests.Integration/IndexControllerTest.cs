using System;
using System.Linq;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class IndexControllerTest
	{
		private static readonly string _fname = "field";

		public sealed partial class EnsureCreated : IClassFixture<BarbadosContextFixture<EnsureCreated>>
		{
			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexCreated);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var before = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);
				_fixture.Context.Database.Indexes.EnsureCreated(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.False(before);
				Assert.True(after);
			}

			[Fact]
			public void ById_CollectionExists_IndexExists_IndexNotCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexNotCreated);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				_fixture.Context.Database.Indexes.Create(collection.Id, _fname);
				var before = _fixture.Context.Database.Indexes.ListIndexed(collection.Id).Count();
				_fixture.Context.Database.Indexes.EnsureCreated(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.ListIndexed(collection.Id).Count();

				Assert.Equal(before, after);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(new ObjectId(-1), _fname)
				);

			}

			[Fact]
			public void ById_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthLessThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ById_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthGreaterThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexCreated);
				_fixture.Context.Database.Collections.Create(cname);
				var before = _fixture.Context.Database.Indexes.Exists(cname, _fname);
				_fixture.Context.Database.Indexes.EnsureCreated(cname, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.False(before);
				Assert.True(after);
			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_IndexNotCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexNotCreated);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var before = _fixture.Context.Database.Indexes.ListIndexed(cname).Count();
				_fixture.Context.Database.Indexes.EnsureCreated(cname, _fname);
				var after = _fixture.Context.Database.Indexes.ListIndexed(cname).Count();

				Assert.Equal(before, after);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(cname, _fname)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(CommonIdentifiers.Collections.MetaCollection, _fname)
				);
			}

			[Fact]
			public void ByName_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthLessThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ByName_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthGreaterThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);
			
				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}
		}

		public sealed partial class EnsureDeleted : IClassFixture<BarbadosContextFixture<EnsureDeleted>>
		{
			[Fact]
			public void ById_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var before = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);
				_fixture.Context.Database.Indexes.EnsureDeleted(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_IndexNotDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexNotDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var before = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);
				_fixture.Context.Database.Indexes.EnsureDeleted(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.False(before);
				Assert.False(after);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.EnsureDeleted(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ById_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureDeleted(new ObjectId(-1), _fname)
				);
			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var before = _fixture.Context.Database.Indexes.Exists(cname, _fname);
				_fixture.Context.Database.Indexes.EnsureDeleted(cname, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexNotDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexNotDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				var before = _fixture.Context.Database.Indexes.Exists(cname, _fname);
				_fixture.Context.Database.Indexes.EnsureDeleted(cname, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.False(before);
				Assert.False(after);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.EnsureDeleted(cname, _fname)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.EnsureDeleted(CommonIdentifiers.Collections.MetaCollection, _fname)
				);
			}
		}

		public sealed partial class Exists : IClassFixture<BarbadosContextFixture<Exists>>
		{
			[Fact]
			public void ById_CollectionExists_IndexExists_ReturnsTrue()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_ReturnsTrue);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var exists = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.True(exists);
			}

			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_ReturnsFalse()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_ReturnsFalse);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var exists = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.False(exists);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Exists(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_ReturnsTrue()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_ReturnsTrue);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var exists = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.True(exists);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_ReturnsFalse()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_ReturnsFalse);
				_fixture.Context.Database.Collections.Create(cname);
				var exists = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.False(exists);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Exists(cname, _fname)
				);
			}
		}

		public sealed partial class Get : IClassFixture<BarbadosContextFixture<Get>>
		{
			[Fact]
			public void ById_CollectionExists_IndexExists_ReturnsIndex()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_ReturnsIndex);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var index = _fixture.Context.Database.Indexes.Get(collection.Id, _fname);

				Assert.Equal(index.IndexField, _fname);
				Assert.Equal(index.CollectionId, collection.Id);
			}

			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_Throws);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Get(collection.Id, _fname)
				);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Get(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Get(new ObjectId(-1), _fname)
				);

			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_ReturnsIndex()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_ReturnsIndex);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var index = _fixture.Context.Database.Indexes.Get(cname, _fname);

				Assert.Equal(index.IndexField, _fname);
				Assert.Equal(index.CollectionId, collection.Id);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Get(cname, _fname)
				);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Get(cname, _fname)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Get(CommonIdentifiers.Collections.MetaCollection, _fname)
				);
			}
		}

		public sealed partial class Create : IClassFixture<BarbadosContextFixture<Create>>
		{
			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexCreated);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				var before = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);
				_fixture.Context.Database.Indexes.Create(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.False(before);
				Assert.True(after);
			}

			[Fact]
			public void ById_CollectionExists_IndexExists_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_Throws);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				_fixture.Context.Database.Indexes.Create(collection.Id, _fname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Create(collection.Id, _fname)
				);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Create(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(new ObjectId(-1), _fname)
				);
			}

			[Fact]
			public void ById_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthLessThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ById_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthGreaterThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexCreated);
				_fixture.Context.Database.Collections.Create(cname);
				var before = _fixture.Context.Database.Indexes.Exists(cname, _fname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.False(before);
				Assert.True(after);
			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_Throws);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname)
				);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(CommonIdentifiers.Collections.MetaCollection, _fname)
				);
			}

			[Fact]
			public void ByName_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthLessThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Fact]
			public void ByName_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthGreaterThanAllowed_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}
		}

		public sealed partial class Delete : IClassFixture<BarbadosContextFixture<Delete>>
		{
			[Fact]
			public void ById_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);
				_fixture.Context.Database.Indexes.Create(collection.Id, _fname);
				var before = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);
				_fixture.Context.Database.Indexes.Delete(collection.Id, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ById_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_Throws);
				_fixture.Context.Database.Collections.Create(cname);
				var collection = _fixture.Context.Database.Collections.Get(cname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Delete(collection.Id, _fname)
				);
			}

			[Fact]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Delete(ObjectId.Invalid, _fname)
				);
			}

			[Fact]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Delete(new ObjectId(-1), _fname)
				);
			}

			[Fact]
			public void ByName_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexDeleted);
				_fixture.Context.Database.Collections.Create(cname);
				_fixture.Context.Database.Indexes.Create(cname, _fname);
				var before = _fixture.Context.Database.Indexes.Exists(cname, _fname);
				_fixture.Context.Database.Indexes.Delete(cname, _fname);
				var after = _fixture.Context.Database.Indexes.Exists(cname, _fname);

				Assert.True(before);
				Assert.False(after);
			}

			[Fact]
			public void ByName_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_Throws);
				_fixture.Context.Database.Collections.Create(cname);

				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Delete(cname, _fname)
				);
			}

			[Fact]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => _fixture.Context.Database.Indexes.Delete(cname, _fname)
				);
			}

			[Fact]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => _fixture.Context.Database.Indexes.Delete(CommonIdentifiers.Collections.MetaCollection, _fname)
				);
			}
		}
	}
}
