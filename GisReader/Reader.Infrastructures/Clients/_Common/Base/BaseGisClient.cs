using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Clients;
using Reader.Abstraction.Clients.Exceptions;
using Reader.Abstraction.Clients.Models.Results;
using Reader.Abstraction.Layers.Models;

namespace Reader.Infrastructures.Clients._Common.Base
{
    public abstract class BaseGisClient : IGisClient
    {
        protected readonly GisClientConfiguration Configuration;
        protected readonly HttpClient Client;
        protected readonly ILogger<BaseGisClient> Logger;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        protected BaseGisClient(HttpClient client, GisClientConfiguration configuration, ILogger<BaseGisClient> logger)
        {
            Client = client;
            Configuration = configuration;
            Logger = logger;

        }
        public async Task<GisObjectIdsResult> GetLayerObjectIdsAsync(GisLayer layer)
        {
            return await GetAsync<GisObjectIdsResult>(GenerateGetLayerObjectIdsUrlAsync(layer));
        }
        protected abstract string GenerateGetLayerObjectIdsUrlAsync(GisLayer layer);

        public async Task<GisLayerInformationResult> GetLayerInformationAsync(GisLayer layer)
        {
            return await GetAsync<GisLayerInformationResult>(GenerateGetLayerInformationUrl(layer));
        }

        protected abstract string GenerateGetLayerInformationUrl(GisLayer layer);

        //public async Task<JsonObject> GetLayerObjectsAsync(int layer, IEnumerable<int>? objectIds = null)
        //{
        //    return await GetAsync<JsonObject>(GenerateGetLayerObjectsUrl(layer, objectIds));
        //}

        public async Task<JsonObject> GetLayerObjectsAsync(GisLayer layer, int startObjectId, int endObjectId, string objectIdFieldName)
        {
            var url = GenerateGetLayerObjectsUrl(layer, startObjectId, endObjectId, objectIdFieldName);
            return await GetAsync<JsonObject>(url);
        }

        public async Task<IReadOnlyList<GisLayer>> GetLayersAsync()
        {
            var layers = await GetAsync<IReadOnlyList<GisLayer>>(GenerateGetLayersUrl(),
                Configuration.LayersIdJsonKey);
            var tables = await GetAsync<IReadOnlyList<GisLayer>>(GenerateGetLayersUrl(),
                Configuration.TablesIdJsonKey);
            var result = new List<GisLayer>();
            result.AddRange(layers);
            result.AddRange(tables);
            return result;
        }

        protected abstract string GenerateGetLayersUrl();
        protected abstract string GenerateGetLayerObjectsUrl(GisLayer layer, int startObjectId, int endObjectId, in string objectIdFieldName);
        //protected abstract string GenerateGetLayerObjectsUrl(int layer, IEnumerable<int>? objectIds = null);

        private async Task<T> GetAsync<T>(string url) where T : JsonNode
        {
            url = AuthenticateUrl(url);
            using var response = await ExecuteAsync(() => Client.GetAsync(url));
            var strContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(strContent, JsonOptions);
            if (result == null)
                throw new FailToParseRequestDataException("Fail to parse json.");

            if (result["error"] != null)
                throw new GisServerErrorException(result.ToJsonString());

            return result;
        }
        protected virtual async Task<T> GetAsync<T>(string url, params string[] paths)
        {
            var result = await GetAsync<JsonNode>(AuthenticateUrl(url));
            foreach (var path in paths)
            {
                result = result[path] ?? throw new InvalidOperationException($"Json does not contain {path}.");
            }

            return result.Deserialize<T>(JsonOptions) ?? throw new Exception("Fail to parse json.");
        }

        private async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> req)
        {
            string? lastErrorResponse = null;
            HttpStatusCode? lastStatusCode = null;
            for (var i = 0; i < Configuration.MaxRetires; i++)
            {
                var result = await req();
                if (result.IsSuccessStatusCode)
                    return result;
                lastErrorResponse = await result.Content.ReadAsStringAsync();
                lastStatusCode = result.StatusCode;
                if (result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    Logger.LogWarning("Fail to process request with status code {statusCode} and response {response}.",
                    lastStatusCode, lastErrorResponse);
                    await Task.Delay(Configuration.RetryDelay);
                    continue;
                }

                throw new FailToExecuteGisRequestException(lastErrorResponse, (int)lastStatusCode);
            }

            if (lastErrorResponse == null || lastStatusCode == null)
                throw new FailToExecuteGisRequestException("Fail to process request");
            throw new FailToExecuteGisRequestException(lastErrorResponse, (int)lastStatusCode);
        }

        public virtual async Task InitializeAsync()
        {
            if (Configuration.UseEsb && Configuration.EsbConfig != null)
            {
                await GetToken();
            }

            //return Task.CompletedTask;
        }

        public virtual string AuthenticateUrl(in string url)
        {
            return url;
        }

        private async Task GetToken()
        {
            try
            {
                var credentials = $"{Configuration.EsbConfig!.Username}:{Configuration.EsbConfig.Password}";
                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                using var request = new HttpRequestMessage(HttpMethod.Post, Configuration.EsbConfig.Url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
                using var body = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content = body;
                using var response = await Client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseContent);
                string accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString() ?? "";
                if (!string.IsNullOrEmpty(accessToken))
                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            catch
            {
            }
        }
    }
}
