using Reader.Abstraction.Clients;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Services;

namespace Reader.Abstraction.Layers.Default
{
    public class GisLayerProvider : IGisLayerProvider
    {
        private readonly IGisClient _client;

        public GisLayerProvider(IGisClientFactory factory , GisServiceConfiguration configuration)
        {
            _client = factory.Create(configuration.Provider);
        }

        public Task<IReadOnlyList<GisLayer>> GetDefaultLayersAsync()
        {
            return _client.GetLayersAsync();
        }
    }
}
