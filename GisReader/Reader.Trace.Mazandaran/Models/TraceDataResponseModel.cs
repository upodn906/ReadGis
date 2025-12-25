using System.Text.Json.Serialization;

namespace Reader.Trace.Mazandaran.Models
{
    public class TraceDataResponseModel
    {
        [JsonPropertyName("gis_table")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("gis_codes")]
        public List<string> GISCodes { get; set; }
    }

}
