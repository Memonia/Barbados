using System;
using System.IO;
using System.Linq;

using Barbados.StorageEngine;
using Barbados.StorageEngine.Documents;

namespace Sandbox
{
	internal sealed class Program
	{
		static void Main(string[] args)
		{
			File.Delete("barbados.db");

			// Our database file is called 'barbados.db'
			using var context = new BarbadosContext("barbados.db", openOrCreate: true);

			// Create a collection
			context.BarbadosController.CreateCollection("users");
			context.BarbadosController.CreateIndex("users", "name");
			var collection = context.BarbadosController.GetCollection("users");

			// Builder is used to create documents
			var documentBuilder = new BarbadosDocument.Builder();

			// Create a few user documents
			var user1FavGame = documentBuilder
				.Add("name", "Genshin " + Enumerable.Repeat("a", 64 * 64))
				.Add("review", "Fix Mona!")
				.Add("reviewScore", (byte)20)
				.Add("hoursPlayed", 17)
				.Build();

			var c = 4096;
			for (int i = 0; i < c; ++i)
			{
				documentBuilder.Add(i.ToString(), user1FavGame);
			}

			var user = documentBuilder.Build();
			var count = 1024;
			for (int i = 0; i < count; ++i)
			{
				collection.Insert(user);
			}

			Console.WriteLine(collection.GetCursor().ToList().Count); 
		}
	}
}