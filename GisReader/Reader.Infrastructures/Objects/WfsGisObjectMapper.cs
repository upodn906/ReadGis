using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects;
using Reader.Abstraction.Objects.Default;
using Reader.Infrastructures.Geometries;

namespace Reader.Infrastructures.Objects;
public class WfsGisObjectMapper<TGisObject> : GisObjectMapper<TGisObject> , IGisObjectMapper where TGisObject : IGisObject, new()
{
    private readonly IGisObjectTransformer _transformer;
    private readonly IGisGeometryFactory _geometryFactory;
    private readonly IGisObjectProcessor _processor;
    private readonly ILogger<WfsGisObjectMapper<TGisObject>> _logger;

    public WfsGisObjectMapper(
        IGisObjectTransformer transformer,
        IGisGeometryFactory geometryFactory,
        IGisObjectProcessor processor,
        ILogger<WfsGisObjectMapper<TGisObject>> logger) : base(geometryFactory)
    {
        _transformer = transformer;
        _geometryFactory = geometryFactory;
        _processor = processor;
        _logger = logger;
    }
    public override IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer)
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


        var featureProperties = node.AsObject()?
            .Where(Q => Q.Value != null && Q.Value.GetValueKind() != JsonValueKind.Object)
            .ToList();
        if (featureProperties != null)
        {
            foreach (var property in featureProperties)
            {
                AddProperty(info, property);
            }
        }

        var properties = node["properties"]?.AsObject();
        if (properties != null)
        {
            foreach (var property in properties)
            {
                AddProperty(info, property);
            }
        }
        var geometry = node?["geometry"];
        if (geometry == null)
        {
            //info.Hash = GenerateHash(info);
            return info;
        }
        var geometryType = geometry["type"]?.GetValue<string>();
        if (geometryType == null)
        {
            //info.Hash = GenerateHash(info);
            return info; 
        }
        MapGeometry(info, geometry, geometryType);
        //info.Hash = GenerateHash(info);
        return info;

        void AddProperty(TGisObject info, KeyValuePair<string, JsonNode?> property)
        {
            var value = FlatValue(property.Value);
            if (value == null)
                return;
            var key = GetKey(property.Key, info.Data);
            info.Data[key] = value;
            if (key == "OID" && value.GetType() == typeof(int))
            {
                //info.ObjectId = (int)value;
            }
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
}