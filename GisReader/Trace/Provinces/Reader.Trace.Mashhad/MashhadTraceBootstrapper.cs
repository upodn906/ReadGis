using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Trace.Ezri;

namespace Reader.Trace.Mashhad
{
    public static class MashhadTraceBootstrapper
    {
        public static void AddServices(IServiceCollection collection , IConfiguration configuration)
        {
            collection.AddSingleton<INetworkTracer, EzriNetworkTracer>();
            collection.AddSingleton<IFeederProvider, EzriFeederProvider>();
            collection.AddSingleton<EzriTraceClient>();
        }
    }
}
