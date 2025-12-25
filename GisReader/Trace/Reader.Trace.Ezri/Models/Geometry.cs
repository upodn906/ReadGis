using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class Geometry
    {
        [JsonPropertyName("paths")]
        public List<List<List<double>>> Paths { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }


}
