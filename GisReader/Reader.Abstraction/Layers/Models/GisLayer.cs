using System.Text.Json.Serialization;
using _Framework.Entities;

namespace Reader.Abstraction.Layers.Models
{
    public record GisLayer : IEntity<int>
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string EnName { get; set; }
    }
}
