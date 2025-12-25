using System.Data.SqlTypes;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Clients;
using Reader.Abstraction.Clients.Models.Results;
using Reader.Abstraction.Layers.Models;
using Reader.Infrastructures.Clients._Common.Base;
using Azure.Core;

namespace Reader.Infrastructures.Clients;

public class WfsJsonClient : IGisClient
{
    private readonly HttpClient _client;
    private readonly GisClientConfiguration _configuration;
    private readonly ILogger<BaseGisClient> _logger;
    private static string? _token;
    private bool _initialized = false;
    public WfsJsonClient(HttpClient client, GisClientConfiguration configuration, ILogger<BaseGisClient> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _client.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<GisObjectIdsResult> GetLayerObjectIdsAsync(GisLayer layer)
    {
        await InitializeAsync();
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&resultType=hits";
        using var response = await GetAsync(url);
        var raw = await response.Content.ReadAsStringAsync();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(raw);

        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsmgr.AddNamespace("wfs", "http://www.opengis.net/wfs/2.0");

        var featureCollectionNode = xmlDoc!.SelectSingleNode("//wfs:FeatureCollection", nsmgr);
        var count = 0;
        if (featureCollectionNode != null)
        {
            var numberMatched = featureCollectionNode!.Attributes!["numberMatched"]!.Value;
            if (numberMatched == null)
                return new GisObjectIdsResult()
                {
                    FieldName = "OId",
                    Ids = new List<int>()
                };
            count = int.Parse(numberMatched);
        }
        var ids = new List<int>(count + 1);
        for (int i = 0; i < count; i++)
        {
            ids.Add(i);
        }
        var info = await GetLayerInformationAsync(layer);
        return new GisObjectIdsResult()
        {
            FieldName = info.Fields.FirstOrDefault()?.Name ?? "Id",
            Ids = ids
        };
    }

    public async Task<GisLayerInformationResult> GetLayerInformationAsync(GisLayer layer)
    {
        await InitializeAsync();
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=1.0.0&request=DescribeFeatureType&typeName={layer.EnName}&outputFormat=application/json";
        using var response = await GetAsync(url);

        var obj = await response.Content.ReadFromJsonAsync<JsonObject>() ??
                   throw new Exception("Fail to create json object.");
        return new GisLayerInformationResult
        {
            Id = -1,
            Name = layer.EnName,
            Fields = obj["featureTypes"]![0]!["properties"]!.AsArray()!
                .Select(Q => new FieldResult
                {
                    Name = Q!["name"]!.GetValue<string>(),
                    Type = Q!["localType"]!.GetValue<string>(),
                    Length = -1,
                }).ToList()
        };
    }

    public async Task<JsonObject> GetLayerObjectsAsync(GisLayer layer, int start, int end, string objectIdFieldName)
    {
        await InitializeAsync();
        var takeCount = end - start + 1;
        //var url =
        //    $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326&Count={takeCount}&startindex={start}&sortBy={objectIdFieldName}";
        var url =
           $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326&Count={takeCount}&startindex={start}";
        //var url =
        //   $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326";

        using var response = await GetAsync(url);

        var raw = await response.Content.ReadAsStringAsync();
        try
        {
            return (JsonObject)JsonObject.Parse(raw)!;
        }
        catch (Exception)
        {
            throw new Exception($"Fail to execute request \r\n | {raw}\r\n URL: {url}");
        }
    }

    public async Task<IReadOnlyList<GisLayer>> GetLayersAsync()
    {
        await InitializeAsync();
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=1.0.0&request=GetCapabilities&srsName=EPSG:4326";
        //$"{_configuration.Address}/ows?service=WFS&acceptversions=2.0.0&request=GetCapabilities&outputFormat=application/json&srsName=EPSG:4326";
        using var response = await GetAsync(url);
        var xmlData = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xmlData);
        var id = 0;
        return doc.Descendants()
            .Where(e => e.Name.LocalName == "FeatureType")
            .Select(ft => ft.Element(XName.Get("Name", "http://www.opengis.net/wfs"))!.Value)
            .OrderBy(Q=> Q)
            .Select(Q => new GisLayer { EnName = Q, Id = id++ })
            .ToList();
    }

    public virtual async Task InitializeAsync()
    {
        if (_initialized)
            return;
        if (_configuration.UseEsb && _configuration.EsbConfig != null)
        {
            await SetToken();
        }
        _initialized = true;
        //return Task.CompletedTask;
    }

    private async Task SetToken()
    {
        if (_token != null)
            return;
        _logger.LogInformation("Start setting token...");
        var credentials = $"{_configuration.EsbConfig!.Username}:{_configuration.EsbConfig.Password}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.EsbConfig.Url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
        using var body = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        request.Content = body;
        using var response = await _client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        var node = JsonNode.Parse(responseContent);
        var accessToken = node!["access_token"]!.GetValue<string>()!;
        //using var jsonDoc = JsonDocument.Parse(responseContent);
        //string accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString() ?? "";
        if (!string.IsNullOrEmpty(accessToken))
        {
            _token = accessToken;
            //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _logger.LogWarning("ESB Token: {token}", accessToken);
        }
    }
    private async Task<HttpResponseMessage> GetAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get , url);
        if (_token != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //_logger.LogCritical(request.ToString());
        return await _client.SendAsync(request);
    }
}