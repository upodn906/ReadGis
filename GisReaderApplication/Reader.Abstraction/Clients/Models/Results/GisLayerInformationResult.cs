using System.Text.Json.Serialization;

namespace Reader.Abstraction.Clients.Models.Results
{
    public record GisLayerInformationResult
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("fields")]
        public List<FieldResult> Fields { get; set; } = new();
    }
}
