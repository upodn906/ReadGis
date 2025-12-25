using Microsoft.Extensions.DependencyInjection;
using Reader.Abstraction.Clients;

namespace Reader.Infrastructures.Clients
{
    public class GisClientFactory : IGisClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public GisClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IGisClient Create(GisProvider provider)
        {
            if (provider == GisProvider.Edsab)
                return _serviceProvider.GetRequiredService<EdsabClient>();

            if (provider == GisProvider.Ezri)
                return _serviceProvider.GetRequiredService<EzriClient>();

            if (provider == GisProvider.WfsJson)
                return _serviceProvider.GetRequiredService<WfsJsonClient>();

            throw new NotImplementedException($"Client {provider} does not exist.");
        }
    }
}
