using System.Diagnostics;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reader.Trace.Nima.Db;
using Reader.Trace.Nima.Db.Entities;
using Reader.Trace.Nima.Models.Configs;
using Reader.Trace.Nima.Models.Results;
using Reader.Trace.Nima.Models.Results.Traces;

namespace Reader.Trace.Nima
{
    public class NimaNetworkTracer : INetworkTracer
    {
        private readonly IDbContextFactory<NimaDbContext> _factory;
        private readonly ILogger<NimaNetworkTracer> _logger;
        private readonly NimaClient _client;

        public NimaNetworkTracer(IDbContextFactory<NimaDbContext> factory , NimaConfig config , ILogger<NimaNetworkTracer> logger)
        {
            _factory = factory;
            _logger = logger;
            _client = new NimaClient(config);
        }


        private async Task CleanTracesAsync(NimaDbContext ctx)
        {
            //await ctx.Database.ExecuteSqlAsync(
            //    FormattableStringFactory.Create("truncate table Edsab.t_Trace"));
            await ctx.TruncateAsync<TraceEntity>();
            await ctx.TruncateAsync<TraceRecordEntity>();
            _logger.LogInformation("Cleared all traces.");
        }

        private async Task UpdateDbSymbolsAsync(NimaDbContext ctx)
        {
            var symbolsCount = await ctx.Symbols.CountAsync();
            if (symbolsCount != 0)
                return;

            var symbols = await _client.GetClassesAsync();
            _logger.LogInformation("Loaded {count} symbols.", symbols.Count);
            var entities = symbols.Select(Q => new SymbolEntity
            {
                ENAME = Q.ENAME,
                FNAME = Q.FNAME,
                FTYPE = Q.FTYPE,
                MaxScale = Q.MaxScale,
                MinScale = Q.MinScale,
                Status = Q.Status,
                VoltazheLevel = Q.VoltazheLevel,
                ClassId = Q.ClassId,
                WebId = Q.WebId
            }).ToList();
            await ctx.BulkInsertAsync(entities);
            _logger.LogInformation($"Saved {symbolsCount} symbols.", symbols.Count);
        }

        private async Task UpdateDbFeedersAsync(List<FeederModel> feeders , NimaDbContext ctx)
        {
            var feederEntities = feeders.Select(Q => new FeederEntity()
            {
                FeederId = Q.Id,
                FeederName = Q.FName,
                FeederObjectId = Q.ObjectId,
                FeederType = Q.FeederType
            }).ToList();
            await ctx.BulkInsertOrUpdateAsync(feederEntities);
            _logger.LogInformation("Saved {count} feeders.", feeders.Count);
        }

        public async Task TraceAsync()
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            await _client.AuthenticateAsync();
            await CleanTracesAsync(ctx);
            var feeders = await _client.GetFeedersAsync();
            _logger.LogInformation("Loaded {count} feeders." , feeders.Count);
            await UpdateDbFeedersAsync(feeders , ctx);
            await UpdateDbSymbolsAsync(ctx);
            var traces = new List<TraceEntity>(feeders.Count);
            var traceRecords = new List<TraceRecordEntity>();
            var count = 1;
            var watch = Stopwatch.StartNew();
            const int threads = 5;
            _logger.LogInformation("Start with {threads} threads.", threads);
            await Parallel.ForEachAsync(feeders, new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }, async (feeder, _) =>
            {
                _logger.LogInformation("Start tracing {id}.", feeder.Id);
                try
                {
                    var trace = await _client.TraceBothAsync(new[] { feeder.Id }, type: feeder.FeederType);
                    lock (traces)
                    {
                        traces.Add(new TraceEntity
                        {
                            FeederId = feeder.Id,
                            TraceJson = trace.Json
                        });
                    }

                    lock (traceRecords)
                    {
                        traceRecords.AddRange(trace.Model.Edges.Select(Q => new TraceRecordEntity
                        {
                            FeatureClassName = Q.FeatureClassName,
                            FeederId = feeder.Id,
                            ObjectId = Q.ObjectId,
                        }));
                        traceRecords.AddRange(trace.Model.Junctions.Select(Q => new TraceRecordEntity
                        {
                            FeatureClassName = Q.FeatureClassName,
                            FeederId = feeder.Id,
                            ObjectId = Q.ObjectId,
                        }));
                        traceRecords.AddRange(trace.Model.Polygons.Select(Q => new TraceRecordEntity
                        {
                            FeatureClassName = Q.FeatureClassName,
                            FeederId = feeder.Id,
                            ObjectId = Q.ObjectId,
                        }));
                    }
                    _logger.LogInformation("Traced {feederId} | {count}/{feedersCount}.", feeder.Id , count , feeders.Count);
                }
                catch (Exception e)
                {
                    _logger.LogError(e ,"Fail to trace feeder {feederId}." , feeder.Id);
                }
                count++;
            });
            await ctx.BulkInsertAsync(traces , new BulkConfig
            {
                BulkCopyTimeout = 300,
            });
            await ctx.BulkInsertAsync(traceRecords, new BulkConfig
            {
                BulkCopyTimeout = 300
            });
            watch.Stop();
            _logger.LogInformation("Saved {feedersCount} in {elapsed}.", feeders.Count, watch.Elapsed);
        }
    }
}