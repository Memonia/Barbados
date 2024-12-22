using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Barbados.Documents.Json
{
	public sealed class BarbadosDocumentJsonConverter : JsonConverter<BarbadosDocument>
	{
		private readonly JsonSerializerOptions _valueOptions;

		public BarbadosDocumentJsonConverter() : this([])
		{

		}

		public BarbadosDocumentJsonConverter(IEnumerable<JsonConverter> valueConverters)
		{
			_valueOptions = new JsonSerializerOptions();
			foreach (var converter in valueConverters)
			{
				_valueOptions.Converters.Add(converter);
			}
		}

		public override BarbadosDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, BarbadosDocument value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			var e = value.GetKeyStringEnumerator();
			while (e.MoveNext())
			{
				writer.WritePropertyName(e.Current);
				if (value.TryGetDocument(e.Current, out var nested))
				{
					Write(writer, nested, options);
				}

				else
				{
					var v = value.Get(e.Current);
					var raw = JsonSerializer.Serialize(v, _valueOptions);
					writer.WriteRawValue(raw, skipInputValidation: true);
				}
			}

			writer.WriteEndObject();
		}
	}
}
