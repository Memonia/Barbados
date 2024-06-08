#### Status
This is a personal project. No consistent development or maintenance is planned for the future.

## About
Barbados is an embedded, single-file document store supporting simple CRUD operations and querying capabilities. Some of the features include:
* BTree-based indexing: each collection has a clustered index by default and may have an unlimited number of non-clustered indexes.
* Concurrency control: supports multiple simultaneous readers or a single writer per collection.
* Nested documents: allows for partial loading and indexing of nested documents at any depth.
* Zero maintenance: supports automatic index rebalancing and automatic page compacting. Free pages are reused on demand. 
* Custom storage format: loading documents into memory does not require a deserialisation step.

## How to use it
Barbados is shipped as two separate NuGet packages: [Barbados.StorageEngine](https://www.nuget.org/packages/Memonia.Barbados.StorageEngine) and [Barbados.QueryEngine](https://www.nuget.org/packages/Memonia.Barbados.QueryEngine). The storage engine provides collections, indexing and documents, while the query engine builds on top of it and provides support for queries. Below is a code snippet highlighting some basic functionality.

```c#
 using Barbados.QueryEngine.Query;
 using Barbados.QueryEngine.Query.Extensions;
 using Barbados.StorageEngine;
 using Barbados.StorageEngine.Documents;

 // Our database file is called 'barbados.db'
 using var context = new BarbadosContext("barbados.db", openOrCreate: true);

 // Create a collection
 context.BarbadosController.CreateCollection("users");
 var collection = context.BarbadosController.GetCollection("users");

 // Builder is used to create documents
 var documentBuilder = new BarbadosDocument.Builder();

 // Create a few user documents
 var user1FavGame = documentBuilder
     .Add("name", "Genshin Impact")
     .Add("review", "Fix Mona!")
     .Add("reviewScore", (byte)20)
     .Add("hoursPlayed", 17)
     .Build();

 var user1 = documentBuilder
     .Add("username", "Gold")
     .Add("email", "jackjoe@example.com")
     .Add("favouriteGame", user1FavGame)
     .Build();

 var user2FavGame = documentBuilder
     .Add("name", "Trackmania")
     .Add("hoursPlayed", 11690)
     .Build();

 var user2OwnedGames = new BarbadosDocument[]
 {
     documentBuilder
         .Add("name", "Cyberpunk 2077")
         .Add("achievments", 45)
         .Add("hoursPlayed", 56)
         .Build(),

     documentBuilder
         .Add("name", "Grand Theft Auto VI")
         .Add("hoursPlayed", 110)
         .Add("inGameNickname", "Goldie")
         .Build()
 };

 var user2 = documentBuilder
     .Add("username", "ddXxXbb")
     .Add("favouriteGame", user2FavGame)
     .Add("ownedGames", user2OwnedGames)
     .Build();

 // Insert the documents into the collection
 collection.Insert(user1);
 collection.Insert(user2);

 // Create an index on the 'hoursPlayed' field in the 'favouriteGame' document
 context.BarbadosController.CreateIndex("users", "favouriteGame.hoursPlayed");

 // Write out all documents in the collection
 foreach (var doc in collection.GetCursor())
 {
     Console.WriteLine(doc);
     Console.WriteLine();
 }

 // Find all users who have played more than 100 hours of their favourite game
 // and get the username of the player and the name of the first game in their games list
 var query = collection.Load()
     .Filter(QueryBuilder.Filters.Gt("favouriteGame.hoursPlayed", 100))
     .Project(QueryBuilder.Projection
         .Include("ownedGames.0.name")
         .Include("username")
     );

 // Write out the result
 foreach (var doc in query.Execute())
 {
     Console.WriteLine(doc);
     Console.WriteLine();
 }
```
