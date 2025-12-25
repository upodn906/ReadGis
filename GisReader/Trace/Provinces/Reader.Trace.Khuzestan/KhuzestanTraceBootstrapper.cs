using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Abstraction.Objects;
using Reader.Infrastructures.Sql;
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
            provider.AddSingleton<INetworkTracer, KhuzestanNetworkTracer>();
        }
    }
}
