using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects;
using Reader.Infrastructures.Geometries;

namespace Reader.Infrastructures.Objects;

public class WfsGisObjectMapper<TGisObject> : IGisObjectMapper where TGisObject : IGisObject, new()
{
    private readonly IGisObjectTransformer _transformer;
    private readonly IGisGeometryFactory _geometryFactory;
    private readonly IGisObjectProcessor _processor;
    private readonly ILogger<WfsGisObjectMapper<TGisObject>> _logger;

    public WfsGisObjectMapper(
        IGisObjectTransformer transformer,
        IGisGeometryFactory geometryFactory,
        IGisObjectProcessor processor,
        ILogger<WfsGisObjectMapper<TGisObject>> logger)
    {
        _transformer = transformer;
        _geometryFactory = geometryFactory;
        _processor = processor;
        _logger = logger;
    }
    public IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer)
    {
        if (obj["features"] is JsonArray array == false)
            throw new Exception("obj does not contain features.");

        var result = new List<TGisObject>(array.Count);
        foreach (var item in array)
        {
            result.Add(ParseJsonToGisObject(item!, layer));
        }

        return (IReadOnlyList<IGisObject>)result;
    }

    private TGisObject ParseJsonToGisObject(JsonNode node, GisLayer layer)
    {
        var info = new TGisObject()
        {
            LayerCode = layer.Id,
            LayerName = layer.EnName
        };
        var properties = node["properties"]?.AsObject();
        if (properties != null)
        {
            foreach (var property in properties)
            {
                AddProperty(info, property);
            }
        }
        var featureProperties = node.AsObject()?
            .Where(Q => Q.Value != null && Q.Value.GetValueKind() != JsonValueKind.Object)
            .ToList();
        if(featureProperties != null)
        {
            foreach (var property in featureProperties)
            {
                AddProperty(info, property);
            }
        }

        MapGeometry(info, node);
        return info;

        void AddProperty(TGisObject info, KeyValuePair<string, JsonNode?> property)
        {
            var value = FlatValue(property.Value);
            if (value == null)
                return;
            info.Data[GetKey(property.Key, info.Data)] = value;
        }
    }
    private string GetKey(string key , IReadOnlyDictionary<string , object?> info , int no = 0)
    {
        string fullKey;
        if(no == 0)
            fullKey = key.ToUpper();
        else
            fullKey = $"{key.ToUpper()}{no}";
        if (info.ContainsKey(fullKey))
            return GetKey(key, info, no + 1);
        return fullKey;

    }
    private void MapGeometry(TGisObject obj, JsonNode? node)
    {
        var geometry = node?["geometry"];
        if (geometry == null)
            return;
        var geometryType = geometry["type"]?.GetValue<string>();
        if (geometryType == null)
            return;
        var geo = _geometryFactory.Create(geometryType, geometry);
        if (geo == null) return;
        obj.ShapeLatLngStr = geo.LatLong.ToString();
        obj.ShapeStr = geo.Utm.ToString();
    }
    public object? FlatValue(object? node)
    {
        if (node == null)
            return null;

        if (node is JsonElement jsonElement == false)
            return node;

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                var dictionary = new Dictionary<string, object?>();
                foreach (var property in jsonElement.EnumerateObject())
                {
                    dictionary[property.Name] = FlatValue(property.Value);
                }

                return dictionary;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var element in jsonElement.EnumerateArray())
                {
                    list.Add(FlatValue(element));
                }

                return list.ToArray();

            case JsonValueKind.String:
                return jsonElement.GetString();

            case JsonValueKind.Number:
                return jsonElement.GetDouble();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();

            case JsonValueKind.Null:
            default:
                return null;
        }
    }
}