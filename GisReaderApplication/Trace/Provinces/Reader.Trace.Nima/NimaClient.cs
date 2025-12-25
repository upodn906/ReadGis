using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HtmlAgilityPack;
using Reader.Trace.Nima.Models.Configs;
using Reader.Trace.Nima.Models.Results;
using Reader.Trace.Nima.Models.Results.Traces;

namespace Reader.Trace.Nima
{
    public class NimaClient
    {
        private readonly NimaConfig _config;
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer = new();
        public NimaClient(NimaConfig config)
        {
            _config = config;
            _client = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = _cookieContainer,
            });
            _client.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task AuthenticateAsync()
        {
            using var login = await _client.GetAsync(_config.Url);
            var html = await login.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var firstTokenTag = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");
            var tokenValue = firstTokenTag.GetAttributeValue("value", "");
            var form = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new("UserName", _config.Username),
                new("Password", _config.Password),
                new("BtnSubmit", ""),
                new("__RequestVerificationToken", tokenValue)
            });
            var loginPostUrl = _config.Url + "Authentication/Login";
            using var loginPost = await _client.PostAsync(loginPostUrl, form);
        }

        public async Task<List<FeederModel>> GetFeedersAsync()
        {
            var url = _config.Url +
                      $"Map/GetMvFeederList?pCompanies={_config.Companies}";
            using var response = await _client.GetAsync(url);
            var result = await response.Content.ReadFromJsonAsync<List<FeederModel>>();
            return result ?? new List<FeederModel>();
        }

        public async Task<FeederTraceModel> TraceAsync(int[] feederIds,
            /*string flowElements = "JunctionsAndEdgesAndPolygon",*/ int type = 0)
        {
          
            var json = await TraceAsJsonAsync(feederIds , type);
            return JsonSerializer.Deserialize<FeederTraceModel>(json) ?? new FeederTraceModel();
        }


        public async Task<(string Json, FeederTraceModel Model)> TraceBothAsync(int[] feederIds,
            /*string flowElements = "JunctionsAndEdgesAndPolygon",*/ int type = 0)
        {
            var json = await TraceAsJsonAsync(feederIds, type);
            var model = JsonSerializer.Deserialize<FeederTraceModel>(json) ?? new FeederTraceModel();
            return (json, model);
        }


        public async Task<string> TraceAsJsonAsync(int[] feederIds,
           /* string flowElements = "JunctionsAndEdgesAndPolygon",*/ int type = 0)
        {
            var request = new
            {
                FeederEndTraceType = type,
                FeederIds = feederIds,
                FlowElements = _config.FlowElements
            };
            var url = _config.Url + "NetworkAnalysis/MvFeederTraceNetwork";
            Exception? lastExp = null;
            for (var i = 0; i < 15; i++)
            {
                try
                {
                    using var response = await _client.PostAsJsonAsync(url, request);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    lastExp = ex;
                    await Console.Out.WriteLineAsync($"Fail to trace | {ex.Message} , Try {i + 1}");
                }
            }

            throw lastExp ?? new Exception($"Fail to trace.");
        }

        public async Task<List<ClassModel>> GetClassesAsync()
        {
            var json = await GetClassesAsJsonAsync();
            return JsonSerializer.Deserialize<List<ClassModel>>(json) ?? new List<ClassModel>();
        }
        public async Task<string> GetClassesAsJsonAsync()
        {
            var url = _config.Url + "Map/GetAllFeatureClassesNameId";
            using var response = await _client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
