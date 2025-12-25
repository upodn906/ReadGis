using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reader.Abstraction;
using Reader.Abstraction.Clients;
using Reader.Abstraction.Layers;
using Reader.Abstraction.Layers.Default;
using Reader.Abstraction.Objects;
using Reader.Abstraction.Objects.Default;
using Reader.Abstraction.Reports;
using Reader.Abstraction.Services;
using Reader.Infrastructures.Clients;
using Reader.Infrastructures.Clients._Common.Base;
using Reader.Infrastructures.Geometries;
using Reader.Infrastructures.Objects;
using Reader.Infrastructures.Sql;
using Reader.Service;

namespace Reader.Infrastructures.Bootstrapper
{
    public static class GisServiceBootstrap
    {
        public static void Bootstrap(IServiceCollection provider , IConfiguration configuration)
        {
            provider.AddSingleton<IConfiguration>(configuration);
            provider.AddSingleton<EzriClient>();
            provider.AddSingleton<EdsabClient>();
            provider.AddSingleton<WfsJsonClient>();
            var gisProvider = configuration["Provider"]?.ToUpper();
            if (gisProvider == null)
                throw new Exception("Provider is null");
            GisProvider providerEnum;
            try
            {
                providerEnum = Enum.GetValues<GisProvider>().First(Q => Enum.GetName(Q)?.ToUpper() == gisProvider);
            }
            catch (Exception e)
            {
                throw new Exception($"Provider {gisProvider} does not exist." , e);
            }
            var batchSizeStr = configuration["BatchSize"];
            if (batchSizeStr == null)
                throw new InvalidOperationException("Batch size is not specefied in config file.");
            var batchSize = int.Parse(batchSizeStr);

            var threadsStr = configuration["Threads"];
            if(threadsStr == null)
                throw new InvalidOperationException("Threads is not specefied in config file.");
            var threads = int.Parse(threadsStr);
            var randomizeStr =  configuration["RandomizeBatches"];
            var randomize = false;
            if (randomizeStr != null)
                randomize = bool.Parse(randomizeStr);
            provider.AddSingleton<GisServiceConfiguration>(_ => new GisServiceConfiguration
            {
                Provider = providerEnum,
                BatchSize = batchSize,
                Threads = threads,
                RandomizeBatches = randomize,
                Layers = configuration.GetSection("Layers")
                ?.Get<HashSet<string>>() ?? new HashSet<string>()
            });
            var gis = new GisClientConfiguration();
            configuration.GetSection("GisServer").Bind(gis);
            if (gis.UseEsb)
            {
                gis.EsbConfig = configuration.GetSection("EsbConfig").Get<EsbConfig>();
            }
            provider.AddSingleton(gis);
            provider.AddSingleton<IGisClientFactory, GisClientFactory>();
            if (providerEnum == GisProvider.WfsJson)
            {
                provider.AddSingleton<IGisGeometryFactory, WfsGeometryFactory>();
                provider.AddSingleton<IGisObjectMapper, WfsGisObjectMapper<GisObject>>();
            }
            else
            {
                provider.AddSingleton<IGisGeometryFactory, EzriGeometryFactory>();
                provider.AddSingleton<IGisObjectMapper, EzriGisObjectMapper<GisObject>>();
            }
            provider.AddSingleton<IGisLayerProvider, GisLayerProvider>();
            provider.AddSingleton<IGisObjectTransformer, GisObjectTransformer>();
            provider.AddSingleton<IGisServiceReport, GisServiceReport>();
            provider.AddSingleton<IGisService, GisService>();
            provider.AddSingleton<IGisObjectProcessor, DefaultObjectProcessor>();

            provider.AddHttpClient("Gis")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
            provider.AddHttpClient<EzriClient>("Gis");
            provider.AddHttpClient<EdsabClient>("Gis");
            provider.AddHttpClient<WfsJsonClient>("Gis");

        }


        public static void AddSqLite(IServiceCollection provider, IConfiguration configuration)
        {
            var conString = configuration.GetConnectionString("Sql");
            provider.AddDbContextFactory<GisDbContext>(opt => 
                opt.UseSqlite(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
            }));
            provider.AddDbContext<GisDbContext>(opt => 
                opt.UseSqlite(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
            }));
            provider.AddSingleton<IGisObjectRepository, SqlGisObjectRepository>();
            provider.AddSingleton<IGisLayerRepository, SqlGisLayerRepository>();
        }

        public static void AddSqlServer(IServiceCollection provider, IConfiguration configuration)
        {
            var conString = configuration.GetConnectionString("Sql");
            provider.AddDbContextFactory<GisDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
                cfg.UseNetTopologySuite();
            }));
            provider.AddDbContext<GisDbContext>(opt => opt.UseSqlServer(conString, cfg =>
            {
                cfg.CommandTimeout(10000000);
                cfg.UseNetTopologySuite();
            }));
            provider.AddSingleton<IGisObjectRepository, SqlGisObjectRepository>();
            provider.AddSingleton<IGisLayerRepository, SqlGisLayerRepository>();
        }
    }
}