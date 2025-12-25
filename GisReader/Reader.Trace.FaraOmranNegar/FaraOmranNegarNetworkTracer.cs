using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Trace;
using Reader.Infrastructures.Clients._Common.Base;
using Reader.Infrastructures.Sql;
using Reader.Trace.FaraOmranNegar.Models;
using RTools_NTS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.FaraOmranNegar
{
    internal class FaraOmranNegarNetworkTracer : INetworkTracer
    {
        private readonly HttpClient _httpClient;
        private readonly GisDbContext _ctx;
        private readonly FaraOmranNegarNetworkTracerConfig _config;
        private readonly GisClientConfiguration _gisConfiguration;
        private readonly ILogger<FaraOmranNegarNetworkTracerConfig> _logger;
        string _token = string.Empty;

        public FaraOmranNegarNetworkTracer(GisDbContext ctx, FaraOmranNegarNetworkTracerConfig configuration, GisClientConfiguration gisConfiguration, ILogger<FaraOmranNegarNetworkTracerConfig> logger)
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

                _logger.LogInformation("Start tracing...");
                //await GetToken();
                //_logger.LogInformation("Trace authentication token is {token}.", _token);


                var ids = _ctx.Objects
                    .Where(Q => Q.LayerCode == _config.FeederLayerCode &&
                                Q.Json != null &&
                                Q.ObjectId != null)
                    .Select(Q => new { Q.Json, Q.ObjectId })
                    .AsEnumerable()
                    .Select(Q => new { JNode = Q.Json != null ? JsonNode.Parse(Q.Json) : null, Q.ObjectId })
                    //.Where(Q => Q.JNode != null && Q.JNode["TYPEE"]?.GetValue<string>() == "MvFeeder")
                    .Select(Q => new { EdsId = Q.JNode!["OBJECTID"]?.GetValue<int>(), CodeOf = Q.JNode["MAPID"]?.GetValue<string>(), Q.ObjectId })
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

                    if (id == null)
                        continue;
                    try
                    {
                        await GetToken();
                        var traceResult = await GetFeederTraceAsync(id.CodeOf.ToString(), _token);
                        //await File.WriteAllTextAsync($"{id}.json", traceResult);
                        //khuCtx.TraceResults.Add(new Db.Entities.TraceResult
                        //{
                        //    FeederObjectId = feederObjId,
                        //    Json = traceResult,
                        //});
                        //await khuCtx.SaveChangesAsync();
                        var result = new List<TraceResult>();

                        foreach (var layer in traceResult.Data)
                        {
                            foreach (var item in layer.GISCodes)
                            {
                                result.Add(new TraceResult
                                {
                                    FeederObjectId = id.ObjectId,
                                    FeederGlobalId = id.CodeOf.ToString(),
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

        private async Task GetToken()
        {
            try
            {
                var userName = Encrypt(_config.Username);
                var password = Encrypt(_config.Password);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_config.TokenUrl}?UserName={userName}&Password={password}&RequestType=19");
                using var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                string accessToken = jsonDoc.RootElement.GetProperty("Data").GetProperty("Token").GetString() ?? "";
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _token = accessToken;
                    _logger.LogInformation("Trace authentication token is {token}.", _token);
                }
                else
                {
                    _logger.LogInformation("Could not get token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("login Faild");
            }
        }

        private async Task<TraceResponseModel> GetFeederTraceAsync(string feederId, string token)
        {
            string url = string.Empty;
            url = $"{_config.TraceUrl}/GetNetworkTracedData";

            //_httpClient.DefaultRequestHeaders.Remove("Authorization");
            //_httpClient.DefaultRequestHeaders.Add("Authorization", token);

            var content = new StringContent(JsonSerializer.Serialize(new
            {
                Token = token,
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

            //var responseContent = await response.Content.ReadAsStringAsync();
            //var test = JsonSerializer.Deserialize<TraceResponseModel>(responseContent);
            //var test2 = await response.Content.ReadFromJsonAsync<TraceResponseModel>();

            return await response.Content.ReadFromJsonAsync<TraceResponseModel>() ??
                throw new Exception("Fail to create response model.");
        }

        static string Encrypt(string toEncrypt)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string key = "FON@321WeBServicE/Pa$$";
            System.Security.Cryptography.MD5CryptoServiceProvider hashmd5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();
            System.Security.Cryptography.TripleDESCryptoServiceProvider tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = System.Security.Cryptography.CipherMode.ECB;
            tdes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            System.Security.Cryptography.ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
    }
}
