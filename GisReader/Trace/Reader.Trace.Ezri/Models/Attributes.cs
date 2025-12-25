using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class Attributes
    {
        [JsonPropertyName("globalid")]
        public string Globalid { get; set; }

        [JsonPropertyName("objectid")]
        public int Objectid { get; set; }
    }


}
