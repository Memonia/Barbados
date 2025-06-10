using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Barbados.Documents.Json
{
	public sealed class DateTimeArrayJsonConverter : JsonConverter<DateTime[]>
	{
		private readonly DateTimeJsonConverter _converter;

		public DateTimeArrayJsonConverter(DateTimeJsonConverter converter)
		{
			_converter = converter;
		}

		public override DateTime[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var list = new List<DateTime>();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
				{
					break;
				}

				var dt = _converter.Read(ref reader, typeof(DateTime), options);
				list.Add(dt);
			}

			return [.. list];
		}

		public override void Write(Utf8JsonWriter writer, DateTime[] value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var item in value)
			{
				_converter.Write(writer, item, options);
			}

			writer.WriteEndArray();
		}
	}
}
