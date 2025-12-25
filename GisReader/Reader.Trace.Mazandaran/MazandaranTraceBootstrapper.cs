using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Infrastructures.Clients._Common.Base;

namespace Reader.Trace.Mazandaran
{
    public static class MazandaranTraceBootstrapper
    {
        public static void AddServices(IServiceCollection provider, IConfiguration configuration)
        {
            var config = configuration.GetSection("Trace")?.Get<MazandaranNetworkTracerConfig>();
            if (config == null)
                throw new InvalidOperationException("Mazandaran tracer config is not registered.");
            var gis = new GisClientConfiguration();
            configuration.GetSection("GisServer").Bind(gis);
            if (gis.UseEsb)
            {
                gis.EsbConfig = configuration.GetSection("EsbConfig").Get<EsbConfig>();
            }
            provider.AddSingleton(config);
            provider.AddSingleton(gis);
            provider.AddSingleton<INetworkTracer, MazandaranNetworkTracer>();
        }
    }
}
