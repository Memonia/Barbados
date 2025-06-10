using System.Text.Json;

namespace Barbados.Documents.Json
{
	public static class BarbadosDocumentJsonExtensions
	{
		private static readonly JsonSerializerOptions _defaultOpts;

		static BarbadosDocumentJsonExtensions()
		{
			var dt = new DateTimeJsonConverter(compact: false);
			var dta = new DateTimeArrayJsonConverter(dt);
			var jsonConverter = new BarbadosDocumentJsonConverter([dt, dta]);

			_defaultOpts = new JsonSerializerOptions()
			{
				Converters = { jsonConverter },
				WriteIndented = true
			};
		}

		public static string ToJson(this BarbadosDocument document)
		{
			return JsonSerializer.Serialize(document, _defaultOpts);
		}
	}
}
