using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Reports;
using Reader.Abstraction.Services;
using Reader.Infrastructures.Bootstrapper;
using Reader.Infrastructures.Sql;
using Reader.Trace;
using Reader.Trace.EdsabV2;
using Reader.Trace.FaraOmranNegar;
using Reader.Trace.Khuzestan;
using Reader.Trace.Mashhad;
using Reader.Trace.Mazandaran;
using Reader.Trace.Nima;
using Reader.Trace.RasaamV2;
using Reader.Trace.Yazd;
using Serilog;

namespace Reader.Console
{
    public class ReaderService
    {
        private DateTime? _lastReadDateTime = null;
        private TimeOnly ReadTime = new(2, 0);
        private List<int> AllowedDays;
        private string _mode;

        public (IServiceProvider, IGisService, SyncService, INetworkTracer) CreateServices()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _mode = config["Mode"]?.ToUpper() ?? "FULL";
            ReadTime = config.GetSection("ReadTime").Get<TimeOnly>();
            AllowedDays = config.GetSection("AllowedDays")?.Get<List<int>>() ?? new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
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
            var province = config["Tracer"]?.Trim().ToUpper() ?? "EMPTY";
            AddTracer(province, serviceCollection, config);
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
                case "EDSAB":
                    EdsabTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "NIMA":
                    NimaTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "MASHHAD":
                    MashhadTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "YAZD":
                    YazdTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "RASAAM":
                    RasaamTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "FARAOMRANNEGAR":
                    FaraOmranNegarTraceBootstrapper.AddServices(serviceCollection, configuration);
                    break;
                case "MAZANDARAN":
                    MazandaranTraceBootstrapper.AddServices(serviceCollection, configuration);
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
                    Log.Information("Start creating database.");
                    var creationResult = await ctx.Database.EnsureCreatedAsync();
                    if (creationResult)
                    {
                        Log.Information("Created database successfuly.");
                    }
                    else
                    {
                        Log.Information("Database already exist.");
                    }
                    //await PerformScanAsync(readService, provider, syncService, tracer);
                    //_lastReadDateTime = DateTime.Now;

                    List<string?> dayNames = AllowedDays
                        .Select(d =>
                        {
                            return Enum.GetName(typeof(DayOfWeek), ((d + 6) % 7));
                        })
                        .ToList();
                    string daysName = string.Join(",", dayNames);

                    Log.Information($"Read start on {ReadTime} every {daysName}");
                    while (true)
                    {
                        await Task.Delay(500);

                        var nowDateTime = DateTime.Now;

                        if (!AllowedDays.Contains(((int)nowDateTime.DayOfWeek + 1) % 7))
                            continue;

                        if (TimeOnly.FromDateTime(nowDateTime) < ReadTime)
                            continue;


                        if (_lastReadDateTime != null &&
                            _lastReadDateTime.Value.Day == nowDateTime.Day)
                            continue;

                        //if (nowDateTime.Hour != ReadTime.Hour)
                        //    continue;


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
                    Log.Information("Read End.");
                }
                if (_mode == "FULL" || _mode == "TRACE")
                {
                    Log.Information("Start to trace");
                    await tracer.TraceAsync();
                    Log.Information("Trace End.");
                }
                if (_mode == "FULL" || _mode == "SYNC" || _mode == "READ")
                {
                    Log.Information("Start to sync");
                    await syncService.ExecuteSyncSpAsync();
                    Log.Information("Sync End.");
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