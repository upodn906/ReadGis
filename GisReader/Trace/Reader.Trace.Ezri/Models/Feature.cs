using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class Feature
    {
        [JsonPropertyName("attributes")]
        public Attributes Attributes { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }
    }


}
