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
			var e = value.GetKeyEnumerator(flat: false);
			while (e.MoveNext())
			{
				var current = e.GetCurrent();
				writer.WritePropertyName(current.ToString());
				if (value.TryGetDocument(current, out var doc))
				{
					Write(writer, doc, options);
				}

				else
				{
					var v = value.Get(current);
					var raw = JsonSerializer.Serialize(v, _valueOptions);
					writer.WriteRawValue(raw, skipInputValidation: true);
				}
			}

			writer.WriteEndObject();
		}
	}
}
