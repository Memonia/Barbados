> [!WARNING]
> This is a personal project. No consistent development or maintenance is planned for the future.

## About  
Barbados is an embedded, ACID-compliant, thread-safe document store. Major features include: write-ahead log, transaction processing, queries, custom storage format and nested documents support.
* **Transaction processing:** each collection or index operation is a part of an automatic transaction or a user-created explicit transaction.
* **Disaster recovery**: transactions rely on WAL (Write-Ahead Log) to avoid data loss.
* **Indexing:** indexes are implemented as B-Trees. Collections are organised as clustered indexes and may have an unlimited number of non-clustered indexes.
* **Concurrency:** collections and indexes support multiple simultaneous readers or a single writer.
* **Nested documents:** fields in nested documents can be indexed and loaded partially just like the top-level fields.
* **Custom storage format:** loading documents in memory does not require a deserialisation step.
* **Queries:** Barbados includes a primitive query engine to provide support for basic data retrieval.

## How to use it
Barbados consists of two separate NuGet packages. [Memonia.Barbados.StorageEngine](https://www.nuget.org/packages/Memonia.Barbados.StorageEngine) is the core package, which exposes full engine capabilities. [Memonia.Barbados.QueryEngine](https://www.nuget.org/packages/Memonia.Barbados.QueryEngine) package wraps low-level functions of the storage engine and exposes a query API. Below is a code snippet highlighting basic functionality.

```c#
using Barbados.QueryEngine.Query;
using Barbados.QueryEngine.Query.Extensions;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Configuration;
using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Transactions;

var connectionSettings = new ConnectionSettingsBuilder()
	.SetDatabaseFilePath("Barbados.db")
	.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
	.Build();

using var context = new BarbadosContext(connectionSettings);

// Create a collection and some indexes
context.Database.Collections.EnsureCreated("users");
context.Database.Indexes.EnsureCreated("users", "username");
context.Database.Indexes.EnsureCreated("users", "favouriteGame.name");
context.Database.Indexes.EnsureCreated("users", "favouriteGame.hoursPlayed");

var users = context.Database.Collections.Get("users");

// Prepare some documents
var documentBuilder = new BarbadosDocument.Builder();
var user1FavGame = documentBuilder
	.Add("name", "Genshin Impact")
	.Add("review", "Fix Mona!")
	.Add("reviewScore", (byte)20)
	.Add("hoursPlayed", 17)
	.Build();

var user2FavGame = documentBuilder
	.Add("name", "Trackmania")
	.Add("hoursPlayed", 11690)
	.Build();

var user1 = documentBuilder
	.Add("username", "Gold")
	.Add("email", "jackjoe@example.com")
	.Add("favouriteGame", user1FavGame)
	.Build();

var user2 = documentBuilder
	.Add("username", "ddXxXbb")
	.Add("favouriteGame", user2FavGame)
	.Add("achievementIds", new int[] { 177091, 177144, 178002 })
	.Build();

// Prepare a transaction which includes a single 'user' collection
var txBuilder = context.Database.CreateTransaction(TransactionMode.ReadWrite)
	.Include(users);

// Insert both documents as a part of an explicit transaction
using (var _ = txBuilder.BeginTransaction())
{
	users.Insert(user1);
	users.Insert(user2);

	context.Database.CommitTransaction();
}

// Write out all documents in the collection
foreach (var doc in users.GetCursor())
{
	Console.WriteLine(doc);
	Console.WriteLine();
}

// Find all users who have played more than 100 hours of their favourite game
// and get the username of the player, the name of their favourite game and the
// id of the first achievement in their achievement list
var query = users.Load()
	.Filter(QueryBuilder.Filters.Gt("favouriteGame.hoursPlayed", 100))
	.Project(QueryBuilder.Projection
		.Include("username")
		.Include("favouriteGame.name")
		.Include("achievementIds.0")
	);

// Check the query execution plan
Console.WriteLine(query.FormatTranslated());
Console.WriteLine();

// Write out the result
foreach (var doc in query.Execute())
{
	Console.WriteLine(doc);
	Console.WriteLine();
}
```
