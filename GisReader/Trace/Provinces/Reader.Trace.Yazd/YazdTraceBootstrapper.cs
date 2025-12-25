using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Infrastructures.Sql;
using Reader.Trace.Ezri;

namespace Reader.Trace.Yazd
{
    public static class YazdTraceBootstrapper
    {
        public static void AddServices(IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddSingleton<INetworkTracer, EzriNetworkTracer>();
            collection.AddSingleton<INetworkTracer, EzriNetworkTracer>();

            collection.AddSingleton<IFeederProvider, FeederProvider>();
            collection.AddSingleton<EzriTraceClient>();
            collection.AddHttpClient<FeederProvider>("Gis");
            var conString = configuration.GetConnectionString("Sql");
            collection.AddDbContextFactory<YazdDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
                cfg.UseNetTopologySuite();
            }));
            collection.AddDbContext<GisDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
                cfg.UseNetTopologySuite();
            }));
            collection.AddScoped<GisDbContext, YazdDbContext>();
        }
    }
}
