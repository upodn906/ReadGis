using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO;
using Reader.Abstraction.Trace;
using Reader.Infrastructures.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Ezri
{
    public class EzriNetworkTracer : INetworkTracer
    {
        private readonly GisDbContext _ctx;
        private readonly EzriTraceClient _client;
        private readonly IFeederProvider _feederProvider;
        private readonly ILogger<EzriNetworkTracer> _logger;

        public EzriNetworkTracer(GisDbContext gisCtx, EzriTraceClient client, IFeederProvider feederProvider,ILogger<EzriNetworkTracer> logger)
        {
            _ctx = gisCtx;
            _client = client;
            _feederProvider = feederProvider;
            _logger = logger;
        }
        public async Task TraceAsync()
        {
            _logger.LogInformation("Start loading feeders.");
            var feeders = await _feederProvider.ProcessFeederAsync();
            _logger.LogInformation("Loaded {count} layers for tracing.", feeders.Count);
            for (int i = 0; i < feeders.Count; i++)
            {
                var feeder = feeders[i];
                _logger.LogInformation("Start OBJECTID: {objectId} | {no}/{total}", feeder.Id, i + 1, feeders.Count);
                try
                {
                    var result = await _client.TraceAsync(feeder.Geometry);
                    var records = new List<TraceResult>();
                    var edges = result.Edges.Select(Q => Q.Features).SelectMany(Q => Q)
                        .Select(Q => Q.Attributes).Select(Q => new TraceResult
                        {
                            FeederId = feeder.Id,
                            GlobalId = Q.Globalid,
                            ObjectId = Q.Objectid
                        }).ToList();
                    records.AddRange(edges);
                    _logger.LogInformation("Loaded {count} edges for feeder {objectId}.", edges.Count, feeder.Id);
                    var junctions = result.Junctions.Select(Q => Q.Features).SelectMany(Q => Q)
                        .Select(Q => Q.Attributes).Select(Q => new TraceResult
                        {
                            FeederId = feeder.Id,
                            GlobalId = Q.Globalid,
                            ObjectId = Q.Objectid,
                        }).ToList();
                    records.AddRange(junctions);
                    _logger.LogInformation("Loaded {count} junctions for feeder {objectId}.", junctions.Count, feeder.Id);
                    await _ctx.BulkInsertOrUpdateAsync(records);
                    _logger.LogInformation("Saved {count} items for feeder {objectId}.", records.Count, feeder.Id);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fail to trace feeder {objectId}.", feeder.Id);
                }
            }
            _logger.LogInformation("Trace complited!");
        }
    }
}
