using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class SpatialReference
    {
        [JsonPropertyName("wkid")]
        public int Wkid { get; set; }

        [JsonPropertyName("latestWkid")]
        public int LatestWkid { get; set; }
    }


}
