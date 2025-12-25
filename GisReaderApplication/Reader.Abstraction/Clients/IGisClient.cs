using System.Text.Json.Nodes;
using Reader.Abstraction.Clients.Models.Results;
using Reader.Abstraction.Layers.Models;

namespace Reader.Abstraction.Clients
{
    public interface IGisClient
    {
        Task<GisObjectIdsResult> GetLayerObjectIdsAsync(GisLayer layer);
        Task<GisLayerInformationResult> GetLayerInformationAsync(GisLayer layer);
        //Task<object> GetLayerInformationAsync(int layer);
        //Task<JsonObject> GetLayerObjectsAsync(int layer, IEnumerable<int>? objectIds = null);
        Task<JsonObject> GetLayerObjectsAsync(GisLayer layer, int startObjectId, int endObjectId , string objectIdFieldName);
        Task<IReadOnlyList<GisLayer>> GetLayersAsync();
        Task InitializeAsync();
    }
}
