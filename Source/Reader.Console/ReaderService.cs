using Reader.Abstraction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Services.Models;
using Reader.Infrastructures.Bootstrapper;
using Serilog;
using Reader.Abstraction.Reports;
using Reader.Trace.Khuzestan;
using SQLitePCL;
using Topshelf.Configurators;
using Reader.Trace;
using Reader.Trace.Nima;
using Reader.Infrastructures.Clients;
using Reader.Infrastructures.Sql;

namespace Reader.Console
{
    public class ReaderService
    {
        private DateTime? _lastReadDateTime = null;
        private static readonly TimeOnly ReadTime = new(2, 0);
        private string _mode;
        public (IServiceProvider, IGisService, SyncService, INetworkTracer) CreateServices()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _mode = config["Mode"]?.ToUpper() ?? "FULL";
            var conString = config.GetConnectionString("Sql");
            var spName = config["SyncSpName"];
            var serviceCollection = new ServiceCollection();
            GisServiceBootstrap.Bootstrap(serviceCollection, config);
            var dbEngine = config["DatabaseEngine"]?.ToUpper();
            if (dbEngine == "SQLSERVER")
            {
                GisServiceBootstrap.AddSqlServer(serviceCollection, config);
            }
            else if (dbEngine == "SQLITE")
            {
                GisServiceBootstrap.AddSqLite(serviceCollection, config);
            }
            else
            {
                throw new InvalidOperationException($"DatabaseEngine must be specefied between [SQLSERVER] or [SQLITE]");
            }
            var province = config["Province"]?.Trim().ToUpper() ?? "EMPTY";
            AddTracer(province , serviceCollection , config);
            serviceCollection.AddSingleton(_ => new SyncService.SyncServiceConfig
            {
                ConnectionString = conString,
                SpName = spName
            });
            serviceCollection.AddSingleton<SyncService>();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            serviceCollection.AddLogging(opt => { opt.AddSerilog(dispose: true); });
            var provider = serviceCollection.BuildServiceProvider();
            var readService = provider.GetRequiredService<IGisService>();
            var syncService = provider.GetRequiredService<SyncService>();
            var traceService = provider.GetRequiredService<INetworkTracer>();
            return (provider, readService, syncService, traceService);
        }

        private static void AddTracer(string province, IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            switch (province)
            {
                case "KHUZESTAN":
                    KhuzestanTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "NIMA":
                    NimaTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                default:
                    serviceCollection.AddSingleton<INetworkTracer, EmptyNetworkTracer>();
                    break;
            }
        }

        public void Run()
        {
            Task.Run(async () =>
            {
                try
                {
                    var (provider, readService, syncService, tracer) = CreateServices();
                    using var scp = provider.CreateScope();
                    var ctx = scp.ServiceProvider.GetRequiredService<GisDbContext>();
                    System.Console.WriteLine("DBG: Creating database!");
                    var res = await ctx.Database.EnsureCreatedAsync();
                    System.Console.WriteLine("DBG: Start scan!");
                    await PerformScanAsync(readService, provider, syncService, tracer);
                    while (true)
                    {
                        await Task.Delay(500);
                        var nowDateTime = DateTime.Now;
                        if (_lastReadDateTime != null &&
                            _lastReadDateTime.Value.Day == nowDateTime.Day)
                            continue;

                        if (nowDateTime.Hour != ReadTime.Hour)
                            continue;

                        _lastReadDateTime = nowDateTime;
                        await PerformScanAsync(readService, provider, syncService, tracer);
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }
            });
        }

        private async Task PerformScanAsync(IGisService readService, IServiceProvider provider,
            SyncService syncService, INetworkTracer tracer)
        {
            var logger = provider.GetRequiredService<ILogger<ReaderService>>();
            logger.LogInformation("Selected mode is {mode}.", _mode);
            try
            {
                if (_mode == "FULL" || _mode == "READ")
                {
                    await ReadAsync(readService);
                    Reporter.Report(provider.GetRequiredService<IGisServiceReport>());
                }
                if (_mode == "FULL" || _mode == "TRACE")
                {
                    await tracer.TraceAsync();
                }
                if (_mode == "FULL" || _mode == "SYNC")
                {
                    await syncService.ExecuteSyncSpAsync();
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Fail to finish reading.");
            }
        }

        private static async Task ReadAsync(IGisService readService)
        {
            await readService.ScanObjectsAsync();
            while (readService.Status != _Framework.Service._Common.ServiceStatus.Finished)
            {
                await Task.Delay(2000);
            }
        }
    }
}