using System;
using System.Linq;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class IndexControllerTest
	{
		private static readonly string _fname = "field";

		public sealed class EnsureCreated : SetupTeardownBarbadosContextTestClass<EnsureCreated>
		{
			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexCreated);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				var before = Context.Database.Indexes.Exists(collection.Id, _fname);
				Context.Database.Indexes.EnsureCreated(collection.Id, _fname);
				var after = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
				});
			}

			[Test]
			public void ById_CollectionExists_IndexExists_IndexNotCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexNotCreated);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				Context.Database.Indexes.Create(collection.Id, _fname);
				var before = Context.Database.Indexes.ListIndexed(collection.Id).Count();
				Context.Database.Indexes.EnsureCreated(collection.Id, _fname);
				var after = Context.Database.Indexes.ListIndexed(collection.Id).Count();

				Assert.That(before, Is.EqualTo(after));
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.EnsureCreated(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(new ObjectId(-1), _fname)
				);

			}

			[Test]
			public void ById_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthLessThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ById_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthGreaterThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexCreated);
				Context.Database.Collections.Create(cname);
				var before = Context.Database.Indexes.Exists(cname, _fname);
				Context.Database.Indexes.EnsureCreated(cname, _fname);
				var after = Context.Database.Indexes.Exists(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
				});
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_IndexNotCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexNotCreated);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var before = Context.Database.Indexes.ListIndexed(cname).Count();
				Context.Database.Indexes.EnsureCreated(cname, _fname);
				var after = Context.Database.Indexes.ListIndexed(cname).Count();

				Assert.That(before, Is.EqualTo(after));
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.EnsureCreated(cname, _fname)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(BarbadosDbObjects.Collections.MetaCollection, _fname)
				);
			}

			[Test]
			public void ByName_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthLessThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ByName_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthGreaterThanAllowed_Throws);
				Context.Database.Collections.Create(cname);
			
				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureCreated(cname, _fname, maxLength)
				);
			}
		}

		public sealed class EnsureDeleted : SetupTeardownBarbadosContextTestClass<EnsureDeleted>
		{
			[Test]
			public void ById_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexDeleted);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var collection = Context.Database.Collections.Get(cname);
				var before = Context.Database.Indexes.Exists(collection.Id, _fname);
				Context.Database.Indexes.EnsureDeleted(collection.Id, _fname);
				var after = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_IndexNotDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexNotDeleted);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				var before = Context.Database.Indexes.Exists(collection.Id, _fname);
				Context.Database.Indexes.EnsureDeleted(collection.Id, _fname);
				var after = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.EnsureDeleted(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ById_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureDeleted(new ObjectId(-1), _fname)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexDeleted);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var before = Context.Database.Indexes.Exists(cname, _fname);
				Context.Database.Indexes.EnsureDeleted(cname, _fname);
				var after = Context.Database.Indexes.Exists(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexNotDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexNotDeleted);
				Context.Database.Collections.Create(cname);
				var before = Context.Database.Indexes.Exists(cname, _fname);
				Context.Database.Indexes.EnsureDeleted(cname, _fname);
				var after = Context.Database.Indexes.Exists(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.EnsureDeleted(cname, _fname)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.EnsureDeleted(BarbadosDbObjects.Collections.MetaCollection, _fname)
				);
			}
		}

		public sealed class Exists : SetupTeardownBarbadosContextTestClass<Exists>
		{
			[Test]
			public void ById_CollectionExists_IndexExists_ReturnsTrue()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_ReturnsTrue);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var collection = Context.Database.Collections.Get(cname);
				var exists = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.That(exists, Is.True);
			}

			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_ReturnsFalse()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_ReturnsFalse);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				var exists = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.That(exists, Is.False);
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Exists(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_ReturnsTrue()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_ReturnsTrue);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var exists = Context.Database.Indexes.Exists(cname, _fname);

				Assert.That(exists, Is.True);
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_ReturnsFalse()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_ReturnsFalse);
				Context.Database.Collections.Create(cname);
				var exists = Context.Database.Indexes.Exists(cname, _fname);

				Assert.That(exists, Is.False);
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Exists(cname, _fname)
				);
			}
		}

		public sealed class Get : SetupTeardownBarbadosContextTestClass<Get>
		{
			[Test]
			public void ById_CollectionExists_IndexExists_ReturnsIndex()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_ReturnsIndex);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var collection = Context.Database.Collections.Get(cname);
				var index = Context.Database.Indexes.Get(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(index.IndexField.ToString(), Is.EqualTo(_fname));
					Assert.That(index.CollectionId, Is.EqualTo(collection.Id));
				});
			}

			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_Throws);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Get(collection.Id, _fname)
				);
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Get(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Get(new ObjectId(-1), _fname)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_ReturnsIndex()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_ReturnsIndex);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var collection = Context.Database.Collections.Get(cname);
				var index = Context.Database.Indexes.Get(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(index.IndexField.ToString(), Is.EqualTo(_fname));
					Assert.That(index.CollectionId, Is.EqualTo(collection.Id));
				});
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_Throws);
				Context.Database.Collections.Create(cname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Get(cname, _fname)
				);
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Get(cname, _fname)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Get(BarbadosDbObjects.Collections.MetaCollection, _fname)
				);
			}
		}

		public sealed class Create : SetupTeardownBarbadosContextTestClass<Create>
		{
			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_IndexCreated);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				var before = Context.Database.Indexes.Exists(collection.Id, _fname);
				Context.Database.Indexes.Create(collection.Id, _fname);
				var after = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
				});
			}

			[Test]
			public void ById_CollectionExists_IndexExists_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_Throws);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				Context.Database.Indexes.Create(collection.Id, _fname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Create(collection.Id, _fname)
				);
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Create(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(new ObjectId(-1), _fname)
				);
			}

			[Test]
			public void ById_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthLessThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ById_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ById_MaxKeyLengthGreaterThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_IndexCreated()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_IndexCreated);
				Context.Database.Collections.Create(cname);
				var before = Context.Database.Indexes.Exists(cname, _fname);
				Context.Database.Indexes.Create(cname, _fname);
				var after = Context.Database.Indexes.Exists(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.False);
					Assert.That(after, Is.True);
				});
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_Throws);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Create(cname, _fname)
				);
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Create(cname, _fname)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(BarbadosDbObjects.Collections.MetaCollection, _fname)
				);
			}

			[Test]
			public void ByName_MaxKeyLengthLessThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthLessThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.MinIndexKeyMaxLength - 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}

			[Test]
			public void ByName_MaxKeyLengthGreaterThanAllowed_Throws()
			{
				var cname = nameof(ByName_MaxKeyLengthGreaterThanAllowed_Throws);
				Context.Database.Collections.Create(cname);

				var maxLength = Constants.IndexKeyMaxLength + 1;
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Create(cname, _fname, maxLength)
				);
			}
		}

		public sealed class Delete : SetupTeardownBarbadosContextTestClass<Delete>
		{
			[Test]
			public void ById_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ById_CollectionExists_IndexExists_IndexDeleted);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);
				Context.Database.Indexes.Create(collection.Id, _fname);
				var before = Context.Database.Indexes.Exists(collection.Id, _fname);
				Context.Database.Indexes.Delete(collection.Id, _fname);
				var after = Context.Database.Indexes.Exists(collection.Id, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ById_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ById_CollectionExists_IndexDoesNotExist_Throws);
				Context.Database.Collections.Create(cname);
				var collection = Context.Database.Collections.Get(cname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Delete(collection.Id, _fname)
				);
			}

			[Test]
			public void ById_CollectionDoesNotExist_Throws()
			{
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Delete(ObjectId.Invalid, _fname)
				);
			}

			[Test]
			public void ById_ReservedCollectionId_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Delete(new ObjectId(-1), _fname)
				);
			}

			[Test]
			public void ByName_CollectionExists_IndexExists_IndexDeleted()
			{
				var cname = nameof(ByName_CollectionExists_IndexExists_IndexDeleted);
				Context.Database.Collections.Create(cname);
				Context.Database.Indexes.Create(cname, _fname);
				var before = Context.Database.Indexes.Exists(cname, _fname);
				Context.Database.Indexes.Delete(cname, _fname);
				var after = Context.Database.Indexes.Exists(cname, _fname);

				Assert.Multiple(() =>
				{
					Assert.That(before, Is.True);
					Assert.That(after, Is.False);
				});
			}

			[Test]
			public void ByName_CollectionExists_IndexDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionExists_IndexDoesNotExist_Throws);
				Context.Database.Collections.Create(cname);

				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Delete(cname, _fname)
				);
			}

			[Test]
			public void ByName_CollectionDoesNotExist_Throws()
			{
				var cname = nameof(ByName_CollectionDoesNotExist_Throws);
				Assert.Throws<BarbadosException>(
					() => Context.Database.Indexes.Delete(cname, _fname)
				);
			}

			[Test]
			public void ByName_ReservedCollectionName_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => Context.Database.Indexes.Delete(BarbadosDbObjects.Collections.MetaCollection, _fname)
				);
			}
		}
	}
}
