## About
Barbados is an embedded, single-file document store with support for simple CRUD operations and querying capabilities.

## Features
This is a pet project and not in any way a comprehensive storage solution. Below is the list of some features.
* BTree-based indexing with one default clustered index per collection and multiple non-clustered indexes 
* Partial document loading and indexing are supported for nested documents at any depth 
* Multiple simultaneous readers or a single writer per collection
* Zero maintnance. Indexes are kept healthy, space occupied by garbage data in pages and deallocated pages are reused on demand
* Documents are stored in a format which doesn't require expensive serialisation/deserialisation steps. Once raw bytes are loaded in memory, the document can be consumed

## How to use it
Anyone who stumbles upon this project is free to try it out! Note that nothing is guaranteed to work.  

Barbados is shipped as two separate NuGet packages: [Barbados.StorageEngine](https://www.nuget.org/packages/Memonia.Barbados.StorageEngine) and [Barbados.QueryEngine](https://www.nuget.org/packages/Memonia.Barbados.QueryEngine). The storage engine provides collections, indexing and documents, while the query engine builds on top of it and provides support for queries. 

Below is a code snippet highlighting some basic functionality.

```c#
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
