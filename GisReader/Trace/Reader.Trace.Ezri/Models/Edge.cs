using System.Text.Json.Serialization;

namespace Reader.Trace.Ezri.Models
{
    public class Edge
    {
        [JsonPropertyName("displayFieldName")]
        public string DisplayFieldName { get; set; }

        [JsonPropertyName("fieldAliases")]
        public FieldAliases FieldAliases { get; set; }

        [JsonPropertyName("geometryType")]
        public string GeometryType { get; set; }

        [JsonPropertyName("spatialReference")]
        public SpatialReference SpatialReference { get; set; }

        [JsonPropertyName("fields")]
        public List<Field> Fields { get; set; }

        [JsonPropertyName("features")]
        public List<Feature> Features { get; set; }

        [JsonPropertyName("layerName")]
        public string LayerName { get; set; }
    }


}
