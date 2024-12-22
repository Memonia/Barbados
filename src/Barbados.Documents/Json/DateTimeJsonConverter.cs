using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Barbados.Documents.Json
{
	public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
	{
		private readonly bool _compact;

		public DateTimeJsonConverter(bool compact)
		{
			_compact = compact;
		}

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (_compact)
			{
				return new DateTime(reader.GetInt64());
			}

			var str = reader.GetString() ?? throw new JsonException();
			return DateTime.ParseExact(str, "O", CultureInfo.InvariantCulture);
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			if (_compact)
			{
				writer.WriteNumberValue(value.Ticks);
				return;
			}

			writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
		}
	}
}
