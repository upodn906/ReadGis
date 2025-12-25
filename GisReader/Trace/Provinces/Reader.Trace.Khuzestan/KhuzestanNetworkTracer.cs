using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Trace;
using Reader.Infrastructures.Sql;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Reader.Trace.Khuzestan
{
    public class KhuzestanNetworkTracer : INetworkTracer
    {
        //private readonly IDbContextFactory<GisDbContext> _gisContextFactory;
        //private readonly IDbContextFactory<KhuzestanDbContext> _khuzestanContextFactory;
        private readonly GisDbContext _ctx;
        private readonly ILogger<KhuzestanNetworkTracer> _logger;
        private readonly HttpClient _httpClient;


        //private const int FeederLayerCode = 2;
        //private const string BaseUrl = "http://192.168.0.101:8082";
        //private const string Username = "pmuser";
        //private const string Password = "pmuser";


        private const int FeederLayerCode = 17;
        private const string BaseUrl = "http://10.62.144.15:6080";
        private const string Username = "maakan";
        private const string Password = "@021";

        public KhuzestanNetworkTracer(GisDbContext ctx ,
            ILogger<KhuzestanNetworkTracer> logger)
        {
            _ctx = ctx;
            _logger = logger;
            _httpClient = new HttpClient();
        }
        public async Task TraceAsync()
        {
            try
            {
                _logger.LogInformation("Start tracing");
                var feederObjectIds = _ctx.Objects
                    .Where(Q => Q.LayerCode == FeederLayerCode &&
                                Q.Json != null &&
                                Q.ObjectId != null)
                    .Select(Q => Q.Json)
                    .AsEnumerable()
                    .Select(Q => Q != null ? JsonNode.Parse(Q) : null)
                    .Where(Q => Q != null && Q["TYPEE"]?.GetValue<string>() == "MvFeeder")
                    .Select(Q => Q["CODEOF"].GetValue<int>())
                    .ToList();
                _logger.LogInformation("Loaded {count} feeders", feederObjectIds.Count);
                var token = await GetTokenAsync();
                await _ctx.TruncateAsync<TraceResult>();
                foreach (var feederObjId in feederObjectIds)
                {
                    try
                    {
                        var traceResult = await GetFeederTraceAsync(feederObjId, token);
                        await File.WriteAllTextAsync($"{feederObjId}.json", traceResult);
                        //khuCtx.TraceResults.Add(new Db.Entities.TraceResult
                        //{
                        //    FeederObjectId = feederObjId,
                        //    Json = traceResult,
                        //});
                        //await khuCtx.SaveChangesAsync();
                        _logger.LogInformation("Traced {id} feeder", feederObjId);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "Failed to trace feeder {id}.", feederObjId);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Fail to perform trace.");
            }
        }

        private async Task<string> GetFeederTraceAsync(int feederId, string token)
        {
            const string url = $"{BaseUrl}/api/GetNetworkTracedData";
            using var response = await _httpClient.PostAsJsonAsync(url, new
            {
                Token = token,
                TraceType = "1",
                MethodType = "List",
                Flags = new[]
                {
                    new
                    {
                        GISTableCode = "Feeder",
                        GISCode = feederId.ToString()
                    }
                }
            });
            return await response.Content.ReadAsStringAsync();
        }
        private async Task<string> GetTokenAsync()
        {
            const string url = $"{BaseUrl}/api/GenerateToken";
            using var response = await _httpClient.PostAsJsonAsync(url, new
            {
                username = Username,
                password = Password,
                requestType = "20"
            });
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Invalid status code {response.StatusCode} | {content}.");
            var node = JsonNode.Parse(content);
            if (node == null)
                throw new Exception("Invalid json result.");
            return node["Token"]!.GetValue<string>();
        }
    }
}