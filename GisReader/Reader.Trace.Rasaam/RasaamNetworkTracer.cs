using Azure.Core;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Trace;
using Reader.Infrastructures.Clients._Common.Base;
using Reader.Infrastructures.Sql;
using Reader.Trace.Rasaam.Models;
using RTools_NTS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Reader.Trace.Rasaam
{
    public class RasaamNetworkTracer : INetworkTracer
    {
        private readonly HttpClient _httpClient;
        private readonly GisDbContext _ctx;
        private readonly RasaamNetworkTracerConfig _config;
        private readonly GisClientConfiguration _gisConfiguration;
        private readonly ILogger<RasaamNetworkTracerConfig> _logger;
        static string token = string.Empty;

        public RasaamNetworkTracer(GisDbContext ctx, RasaamNetworkTracerConfig configuration, GisClientConfiguration gisConfiguration, ILogger<RasaamNetworkTracerConfig> logger)
        {
            _httpClient = new HttpClient();
            _ctx = ctx;
            _config = configuration;
            _gisConfiguration = gisConfiguration;
            _logger = logger;
        }
        public async Task TraceAsync()
        {
            try
            {
                if (_gisConfiguration.UseEsb && _gisConfiguration.EsbConfig != null)
                {
                    await GetToken();
                }

                _logger.LogInformation("Start tracing...");
                //string token = string.Empty;
                if (!_gisConfiguration.UseEsb)
                {
                    token = await GetTokenAsync();
                    _logger.LogInformation("Trace authentication token is {token}.", token);
                }
                else
                {
                    _logger.LogInformation("Skip Trace authentication (ESB Authentication)");
                }
                var ids = _ctx.Objects
                    .Where(Q => Q.LayerCode == _config.FeederLayerCode &&
                                Q.Json != null &&
                                Q.ObjectId != null)
                    .Select(Q => new { Q.Json, Q.ObjectId })
                    .AsEnumerable()
                    .Select(Q => new { JNode = Q.Json != null ? JsonNode.Parse(Q.Json) : null, Q.ObjectId })
                    //.Where(Q => Q.JNode != null && Q.JNode["TYPEE"]?.GetValue<string>() == "MvFeeder")
                    .Select(Q => new { EdsId = Q.JNode!["SMID"]?.GetValue<int>(), CodeOf = Q.JNode["FEEDER_CODE"]?.GetValue<string>(), Q.ObjectId })
                    .ToList();
                _logger.LogInformation("Loaded {count} feeders for trace.", ids.Count);
                await _ctx.TruncateAsync<TraceResult>();
                var bulkConfig = new BulkConfig
                {
                    SetOutputIdentity = false,
                    WithHoldlock = false,
                };
                var count = 0;
                foreach (var id in ids)
                {
                    count++;
                    _logger.LogInformation($"Trace feeder {count} of {ids.Count}");

                    if (id == null || id.CodeOf == null)
                        continue;
                    try
                    {
                        var traceResult = await GetFeederTraceAsync(id.CodeOf.ToString(), token);
                        //await File.WriteAllTextAsync($"{id}.json", traceResult);
                        //khuCtx.TraceResults.Add(new Db.Entities.TraceResult
                        //{
                        //    FeederObjectId = feederObjId,
                        //    Json = traceResult,
                        //});
                        //await khuCtx.SaveChangesAsync();
                        var result = new List<TraceResult>();
                        foreach (var layer in traceResult.Data.data.Where(x => x.GISCodes != null && x.GISCodes.Count() > 0 && !string.IsNullOrWhiteSpace(x.GISTableCode)))
                        {
                            foreach (var item in layer.GISCodes)
                            {
                                result.Add(new TraceResult
                                {
                                    FeederObjectId = id.ObjectId,
                                    FeederGlobalId = id.EdsId.ToString(),
                                    LayerName = layer.GISTableCode,
                                    GlobalId = item
                                });
                            }
                        }
                        await _ctx.BulkInsertAsync(result, bulkConfig);
                        _logger.LogInformation("Traced {id} feeder.", id);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(e, "Failed to trace feeder {id}.", id);
                    }
                }
                _logger.LogInformation("Successfuly traced all feeders.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Fail to perform network trace.");
            }
        }
        private async Task<TraceResponseModel> GetFeederTraceAsync(string feederId, string token)
        {
            string url = string.Empty;
            if (_gisConfiguration.UseEsb)
                url = _config.Url;
            else
                url = $"{_config.Url}/pm/GetNetworkTracedData";


            if (string.IsNullOrWhiteSpace(token))
                token = await GetToken();
            _logger.LogCritical($"Token = {token}");

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            var content = new StringContent(JsonSerializer.Serialize(new
            {
                TraceType = "1",
                MethodType = "List",
                Flags = new[]
            {
                    new
                    {
                        GISTableCode = "MV_Feeder",
                        GISCode = feederId.ToString()
                    }
                }
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            return await response.Content.ReadFromJsonAsync<TraceResponseModel>() ??
                throw new Exception("Fail to create response model.");
        }
        private async Task<string> GetTokenAsync()
        {
            var url = $"{_config.Url}/GenerateToken";
            using var response = await _httpClient.PostAsJsonAsync(url, new
            {
                username = _config.Username,
                password = _config.Password,
                //requestType = "20"
            });
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Invalid status code {response.StatusCode} | {content}.");
            var node = JsonNode.Parse(content);
            if (node == null)
                throw new Exception("Invalid json result.");
            return node["token"]!.GetValue<string>();
        }
        private async Task<string> GetToken()
        {
            try
            {
                var credentials = $"{_gisConfiguration.EsbConfig!.Username}:{_gisConfiguration.EsbConfig.Password}";
                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                using var request = new HttpRequestMessage(HttpMethod.Post, _gisConfiguration.EsbConfig.Url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
                using var body = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content = body;
                using var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                string accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString() ?? "";
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogInformation("Trace ESB authentication token is {token}.", accessToken);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                else
                {
                    _logger.LogInformation("Could not get ESB authentication");
                }
                return accessToken;
            }
            catch
            {
                return "";
            }
        }
    }
}
