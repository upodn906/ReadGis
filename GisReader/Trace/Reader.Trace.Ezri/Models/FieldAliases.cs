using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class FieldAliases
    {
        [JsonPropertyName("globalid")]
        public string Globalid { get; set; }

        [JsonPropertyName("objectid")]
        public string Objectid { get; set; }
    }


}
