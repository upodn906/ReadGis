using System.Text.Json.Serialization;

namespace Reader.Abstraction.Clients.Models.Results
{
    public record GisObjectIdsResult
    {
        [JsonPropertyName("objectIdFieldName")]
        public string FieldName { get; set; }
        [JsonPropertyName("objectIds")]
        public List<int>? Ids { get; set; }
    }
}
