using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Abstraction.Objects;
using Reader.Infrastructures.Sql;
using Reader.Trace.Khuzestan.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Khuzestan
{
    public static class KhuzestanTraceBootstrapper
    {
        public static void AddServices(IServiceCollection provider, IConfiguration configuration)
        {
            var conString = configuration.GetConnectionString("Sql");
            provider.AddDbContextFactory<KhuzestanDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
            }));
            provider.AddDbContext<KhuzestanDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
            }));
            provider.AddSingleton<INetworkTracer, KhuzestanNetworkTracer>();
        }
    }
}
