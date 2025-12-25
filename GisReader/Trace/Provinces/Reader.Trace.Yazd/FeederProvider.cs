using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Reader.Trace.Ezri;
using Reader.Trace.Ezri.Models;
using Reader.Trace.Yazd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Reader.Trace.Yazd
{
    public class FeederProvider : IFeederProvider
    {
        private readonly string _url;
        private readonly HttpClient _client;
        private readonly YazdDbContext _ctx;
        private readonly ILogger<FeederProvider> _logger;

        public FeederProvider(YazdDbContext ctx , /*HttpClient client,*/ IConfiguration configuration , ILogger<FeederProvider> logger)
        {
            var rayaFeederUrl = configuration["Yazd:FeedersUrl"];
            if (rayaFeederUrl == null)
                throw new Exception("Yazd require a url for proving feeders list in config file.");
            _url = rayaFeederUrl;

            this._ctx = ctx;
            //_client = client;
            _client = new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
            });
            _client.Timeout = TimeSpan.FromMinutes(5);
            _logger = logger;
        }
        public async Task<IReadOnlyList<FeederTraceModel>> ProcessFeederAsync()
        {
            _logger.LogInformation("Start loading feeders from api.");
            using var response = await _client.GetAsync(_url);
            response.EnsureSuccessStatusCode();
            var rawJson = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(rawJson);
            if (node == null)
                throw new Exception("Fail to convert response json into node.");
            var feeders = node!["data"]!.Deserialize<List<Feeder>>();
            if (feeders == null)
                throw new Exception("Fail to extract feeders from response.");
            _logger.LogInformation("Loaded {count} feeders from api.", feeders.Count);
            await _ctx.BulkInsertOrUpdateOrDeleteAsync(feeders!);
            _logger.LogInformation("Saved {count} feeders.", feeders.Count);
            var factory = new GeometryFactory();
            return feeders.Where(Q => Q.X != null && Q.Y != null).Select(Q => new FeederTraceModel
            {
                Id = Q.Id,
                Geometry = factory.CreatePoint(new Coordinate
                {
                    X = double.Parse(Q.X!.Trim()),
                    Y = double.Parse(Q.Y!.Trim())
                })
            }).ToList();
        }
    }
}
