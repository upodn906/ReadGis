using System.Text.Json.Serialization;

namespace Reader.Trace.Rasaam.Models
{
    public class TraceDataResponseModel
    {
        [JsonPropertyName("GISTableCode")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("length")]
        public double Length { get; set; }

        [JsonPropertyName("GISCodes")]
        public List<string> GISCodes { get; set; }
    }

}
