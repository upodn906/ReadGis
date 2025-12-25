using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.AccessControl;
using _Framework.Service._Common;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Clients;
using Reader.Abstraction.Clients.Models.Results;
using Reader.Abstraction.Layers;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects;
using Reader.Abstraction.Reports;
using Reader.Abstraction.Reports.Models;
using Reader.Abstraction.Services;
using Reader.Abstraction.Services.Models;

namespace Reader.Service
{
    public class GisService : IGisService
    {
        private static readonly ServiceResult ScanSuccessfullyStarted
            = ServiceResult.Success("اسکن با موفقیت آغاز شد.");

        private static readonly ServiceResult ScanIsAlreadyRunning =
            ServiceResult.Fail("درحال حاضر یک اسکن در حال اجرا می باشد." +
                               "\r\nبرای انجام اسکن جدید ابتدا باید اسکن درحال اجرا را متوقف کنید.");



        private readonly IGisClient _client;
        private readonly IGisLayerProvider _layerProvider;
        private readonly IGisObjectMapper _objectMapper;
        private readonly IGisObjectRepository _objectRepository;
        private readonly IGisLayerRepository _layerRepository;
        private readonly IGisServiceReport _serviceReport;
        private readonly IGisObjectProcessor _processor;

        //private readonly IGisServiceReportRepository _reportRepository;
        private readonly ILogger<GisService> _logger;

        public GisService(
            GisServiceConfiguration configuration ,
            IGisClientFactory clientFactory,
            IGisLayerProvider layerProvider,
            IGisObjectMapper objectMapper,
            IGisObjectRepository objectRepository,
            IGisLayerRepository layerRepository,
            IGisServiceReport serviceReport,
            IGisObjectProcessor processor,
            ILogger<GisService> logger)
        {
            _client = clientFactory.Create(configuration.Provider);
            _layerProvider = layerProvider;
            _objectMapper = objectMapper;
            _objectRepository = objectRepository;
            _layerRepository = layerRepository;
            _serviceReport = serviceReport;
            _processor = processor;
            _logger = logger;
            Configuration = configuration;
        }

        public GisServiceConfiguration Configuration { get; }
        public ServiceStatus Status { get; private set; } = ServiceStatus.Idle;

        public async Task<ServiceResult> ScanAllLayersObjectsAsync(ScanOptions options)
        {
            return await TryScanAsync(async () =>
            {
                var layers = await GetLayersAndUpdateBaseDataAsync(options);
                var _  = Task.Run(async ()=> await ScanAsync(layers, options));
                return ScanSuccessfullyStarted;
            });
        }
        private async Task<ServiceResult> TryScanAsync(Func<Task<ServiceResult>> func)
        {
            if (IsScanAllowed() == false)
                return ScanIsAlreadyRunning;
            var serviceRes = await func();
            if (serviceRes.IsSuccess == false)
                return serviceRes;
            Status = ServiceStatus.Running;
            return serviceRes;
        }
        private async Task ScanAsync(IReadOnlyList<GisLayer> layers, ScanOptions options)
        {
            if (layers.Count == 0)
            {
                _logger.LogWarning("Scan canceled because 0 layer(s) supplied to system.");
                return;
            }

            _logger.LogInformation("Start gis scan with {threads} threads." , options.Threads);
            await _layerRepository.UpdateAllAsync(layers);
            _logger.LogInformation("Updated layers.");
            await _processor.InitializeAsync();
            await _client.InitializeAsync();
            _serviceReport.Initialize();
            _serviceReport.AddEvent(GisServiceEvent.ApplicationStart());
            _serviceReport.AddEvent(GisServiceEvent.LoadedLayers(layers.Count));
            try
            {
                _serviceReport.InitializeLayers(layers.Select(Q => new GisLayerStats(Q.Id,
                    Q.EnName)));
                var objectIds = await InitializeObjectIdsAsync(layers, options);
                if (objectIds.Count == 0 || objectIds.All(Q => Q.Value.Ids == null || Q.Value.Ids.Count == 0))
                {
                    _logger.LogWarning("Scan canceled because 0 id(s) received from gis.");
                    return;
                }
                await _objectRepository.CleanAsync();
                _logger.LogInformation("Cleaned gis scan.");
                _serviceReport.AddEvent(GisServiceEvent.CleanedOldTemps());
                await ProcessCodesAsync(objectIds , options);
                _logger.LogInformation("Successfully scanned gis service.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Fail to finish process." +
                                       "\r\nStopping process.");
                _serviceReport.AddError(GisServiceError.ErrorInReadingAllLayers(e.Message));
            }
            _serviceReport.Completed();
            _serviceReport.AddEvent(GisServiceEvent.ApplicationFinished());


        }
        private async Task<ConcurrentDictionary<GisLayer, GisObjectIdsResult>>
            InitializeObjectIdsAsync(IReadOnlyList<GisLayer> codes , ScanOptions options)
        {
            _logger.LogInformation("Start loading object ids.");
            _serviceReport.AddEvent(GisServiceEvent.StartLoadingLayersObjectsCount());
            var result = new ConcurrentDictionary<GisLayer, GisObjectIdsResult>();
            await Parallel.ForEachAsync(
                codes, new ParallelOptions() { MaxDegreeOfParallelism = options.Threads }, async (layer, _) =>
                {
                    try
                    {
                        var ids = await _client.GetLayerObjectIdsAsync(layer);
                        if (ids.Ids == null)
                            return;
                        _serviceReport.InitializeLayerCount(layer.Id, ids.Ids.Count);
                        result[layer] = ids;
                        _logger.LogInformation("Loaded {count} object ids for layer [{layerName} - {layerId}]",
                            ids.Ids.Count, layer.EnName, layer.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fail to read layer object ids.");
                        _serviceReport.AddError(
                            GisServiceError.ErrorInReadingLayerCount(layer.Id ,layer.EnName , ex.Message));
                    }
                });
            _serviceReport.AddEvent(GisServiceEvent.EndLoadingLayersObjectsCount());
            return result;
        }
        private async Task ProcessCodesAsync(ConcurrentDictionary<GisLayer, GisObjectIdsResult> ids , ScanOptions options)
        {
            var batches = CreateGisBatches(ids , options);
            _logger.LogInformation("Created {count} gis batches.", batches.Count);
            await Parallel.ForEachAsync(
                batches, new ParallelOptions() { MaxDegreeOfParallelism = options.Threads }, async (batch, _) =>
                {
                    try
                    {
                        var watch = Stopwatch.StartNew();
                        int startObjectId = 0;
                        int endObjectId = int.MaxValue;
                        if (batch.Value.Count != 0)
                        {
                            startObjectId = batch.Value[0];
                            endObjectId = batch.Value[^1];
                        }
                        var gisResult = await _client.GetLayerObjectsAsync(batch.Key.Layer, startObjectId , endObjectId , batch.Key.ObjectIdFiledName);
                        var objects = _objectMapper.Map(gisResult, batch.Key.Layer);
                        watch.Stop();
                        await _objectRepository.AddRangeAsync(objects);
                        _serviceReport.Scanned(batch.Key.Layer.Id, objects.Count, watch.Elapsed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Fail to parse objects layer {layer}." , batch.Key.Layer.Id);
                        _serviceReport.AddError(
                            GisServiceError.ErrorInReadingLayer(batch.Key.Layer.EnName , ex.Message));
                    }
                });
            await _objectRepository.FlushAsync();
            Status = ServiceStatus.Finished;
        }
        private static IReadOnlyList<KeyValuePair<(GisLayer Layer , string ObjectIdFiledName), IReadOnlyList<int>>> CreateGisBatches(
            ConcurrentDictionary<GisLayer, GisObjectIdsResult> ids , ScanOptions options)
        {
            var gisBatches = new List<KeyValuePair<(GisLayer Layer, string ObjectIdFiledName), IReadOnlyList<int>>>();
            foreach (var layer in ids)
            {
                if (layer.Value.Ids?.Count <= options.ReadBatchSize)
                {
                    gisBatches.Add(new KeyValuePair<(GisLayer Layer, string ObjectIdFiledName), IReadOnlyList<int>>(
                        (layer.Key, layer.Value.FieldName), layer.Value.Ids.OrderBy(Q => Q).ToList()));
                    continue;
                }

                gisBatches.AddRange(layer.Value.Ids.OrderBy(Q=>Q).Chunk(options.ReadBatchSize)
                    .Select(Q => new KeyValuePair<(GisLayer Layer, string ObjectIdFiledName), IReadOnlyList<int>>(
                        (layer.Key, layer.Value.FieldName), Q)));
            }
            return gisBatches.OrderBy(Q=>Q.Key.Layer.Id).ToArray();
        }
        private async Task<IReadOnlyList<GisLayer>> GetLayersAndUpdateBaseDataAsync(ScanOptions options)
        {
            await _client.InitializeAsync();
            var systemLayers = await _layerProvider.GetDefaultLayersAsync();
            _logger.LogInformation("Loaded {count} layers.", systemLayers.Count);
            if (Configuration.Layers.Count == 0)
                return systemLayers;
            return systemLayers.Where(Q=> Configuration.Layers
            .Contains(Q.EnName) == true || Configuration.Layers.Contains(Q.Id.ToString())).ToList();
        }
        private bool IsScanAllowed()
        {
            if (Status == ServiceStatus.Running)
                return false;
            return true;
        }
    }
}
