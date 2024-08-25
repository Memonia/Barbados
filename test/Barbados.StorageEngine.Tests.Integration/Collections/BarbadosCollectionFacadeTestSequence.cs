using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Barbados.StorageEngine.Documents;

using Xunit.Abstractions;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	public sealed class BarbadosCollectionFacadeTestSequence(string name, IEnumerable<BarbadosDocument> documents) : IXunitSerializable
	{
		private static readonly JsonSerializerOptions _options = new()
		{
			IncludeFields = true,
		};

		public string Name { get; private set; } = name;
		public IEnumerable<BarbadosDocument> Documents { get; private set; } = documents;

		public BarbadosCollectionFacadeTestSequence() : this(default!, default!)
		{

		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(Name), Name);

			var json = JsonSerializer.Serialize(
				Documents.Select(e => (e.Id.Value, e.Buffer.AsReadonlySpan().ToArray())), _options
			);
			info.AddValue(nameof(Documents), json);
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			Name = info.GetValue<string>(nameof(Name));

			var json = info.GetValue<string>(nameof(Documents));
			Documents = JsonSerializer.Deserialize<(long id, byte[] buffer)[]>(json, _options)!
				.Select(e => new BarbadosDocument(new(e.id), new(e.buffer)))
				.ToArray();
		}

		public override string ToString() => Name;
	}
}
