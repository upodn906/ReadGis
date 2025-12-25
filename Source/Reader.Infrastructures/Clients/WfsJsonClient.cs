using System.Data.SqlTypes;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Clients;
using Reader.Abstraction.Clients.Models.Results;
using Reader.Abstraction.Layers.Models;
using Reader.Infrastructures.Clients._Common.Base;

namespace Reader.Infrastructures.Clients;

public class WfsJsonClient : IGisClient
{
    private readonly HttpClient _client;
    private readonly GisClientConfiguration _configuration;
    private readonly ILogger<BaseGisClient> _logger;

    public WfsJsonClient(HttpClient client, GisClientConfiguration configuration, ILogger<BaseGisClient> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _client.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<GisObjectIdsResult> GetLayerObjectIdsAsync(GisLayer layer)
    {
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&resultType=hits";
        using var response = await _client.GetAsync(url);
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
            if(numberMatched == null)
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
            FieldName =info.Fields.FirstOrDefault()?.Name ?? "Id",
            Ids = ids
        };
    }

    public async Task<GisLayerInformationResult> GetLayerInformationAsync(GisLayer layer)
    {
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=1.0.0&request=DescribeFeatureType&typeName={layer.EnName}&outputFormat=application/json";
        using var response = await _client.GetAsync(url);

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
        var takeCount = end - start + 1;
        //var url =
        //    $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326&Count={takeCount}&startindex={start}&sortBy={objectIdFieldName}";
        //var url =
        //   $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326&Count={takeCount}&startindex={start}";
        var url =
           $"{_configuration.Address}/ows?service=WFS&version=2.0.0&request=GetFeature&typeName={layer.EnName}&outputFormat=application/json&srsName=EPSG:4326";

        using var response = await _client.GetAsync(url);

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
        var url =
            $"{_configuration.Address}/ows?service=WFS&version=1.0.0&request=GetCapabilities&outputFormat=application/json&srsName=EPSG:4326";
        using var response = await _client.GetAsync(url);
        var xmlData = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xmlData);
        var id = 0;
        return doc.Descendants()
            .Where(e => e.Name.LocalName == "FeatureType")
            .Select(ft => ft.Element(XName.Get("Name", "http://www.opengis.net/wfs"))!.Value)
            .Select(Q=> new GisLayer { EnName = Q , Id = id++})
            .ToList();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}