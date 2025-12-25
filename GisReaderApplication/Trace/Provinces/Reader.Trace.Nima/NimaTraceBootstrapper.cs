using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Trace.Nima.Db;
using Reader.Trace.Nima.Models.Configs;

namespace Reader.Trace.Nima
{
    public static class NimaTraceBootstrapper
    {
        public static void AddServices(IServiceCollection provider, IConfiguration configuration)
        {
            var conString = configuration.GetConnectionString("Sql");
            provider.AddDbContextFactory<NimaDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(12000000);
            }));
            provider.AddDbContext<NimaDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(12000000);
            }));
            provider.AddSingleton(configuration.GetSection("Nima").Get<NimaConfig>() ??
                                  throw new Exception("Nima config is invalid."));
            provider.AddSingleton<INetworkTracer, NimaNetworkTracer>();
        }
    }
}
