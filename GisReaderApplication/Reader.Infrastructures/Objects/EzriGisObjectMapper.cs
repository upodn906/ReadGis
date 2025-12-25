using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using ProjNet.CoordinateSystems;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects;
using Reader.Infrastructures.Geometries;
using Reader.Infrastructures.Geometries.Models;

namespace Reader.Infrastructures.Objects
{
    public class EzriGisObjectMapper<TGisObject> : IGisObjectMapper where TGisObject : IGisObject , new()
    {
        private readonly IGisObjectTransformer _transformer;
        private readonly IGisGeometryFactory _geometryFactory;
        private readonly IGisObjectProcessor _processor;
        private readonly ILogger<EzriGisObjectMapper<TGisObject>> _logger;

        public EzriGisObjectMapper(
            IGisObjectTransformer transformer,
            IGisGeometryFactory geometryFactory ,
            IGisObjectProcessor processor,
            ILogger<EzriGisObjectMapper<TGisObject>> logger)
        {
            _transformer = transformer;
            _geometryFactory = geometryFactory;
            _processor = processor;
            _logger = logger;
        }
        private TGisObject ParseJsonToGisObject(JsonNode obj, GisLayer layer, string? geometryType)
        {
            if (obj["attributes"] is not JsonObject attributes)
                throw new Exception($"obj does not contain attributes.");

            var info = new TGisObject()
            {
                LayerCode = layer.Id,
                LayerName =  layer.EnName
            };
            foreach (var prop in attributes)
            {
                var key = GetNameLastSegment(prop.Key);
                var value = prop.Value;
                if (value == null)
                    continue;
                var flatValue = FlatValue(prop.Value?.GetValue<object>());
                if (flatValue != null)
                    info.Data.TryAdd(_transformer.GetFieldName(key), flatValue);
            }
            _processor.ProcessAsync(info);
            if(geometryType != null)
            {
                if (obj["geometry"] is JsonObject geometry)
                {
                    MapGeometry(geometry, info, geometryType);
                }
                else
                {
                    _logger.LogWarning("Object doesnt have geometry.");
                }
            }
            return info;
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

        private static string GetNameLastSegment(in string key)
        {
            var split = key.Split('.');
            if (split.Length <= 1)
                return key.ToUpper();
            return split[^1].ToUpper();
        }

        private void MapGeometry(JsonObject? geometry, TGisObject obj, in string geometryType)
        {
            if (geometry == null)
                return;
            var geo = _geometryFactory.Create(geometryType, geometry);
            if(geo == null) return;
            obj.ShapeLatLngStr = geo.LatLong.ToString();
            obj.ShapeStr = geo.Utm.ToString();
            OnGeometryMapping(geo, obj);
        }

        protected virtual void OnGeometryMapping(GeometryResult geometry, TGisObject obj)
        {

        }
        public IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer)
        {
            if (obj["features"] is not JsonArray features)
                throw new Exception($"obj does not contain features.");

            var geometryType = obj["geometryType"]?.ToString();
            //if (geometryType == null)
            //    throw new Exception("Fail read geometry type.");

            var result = new List<TGisObject>(features.Count);
            foreach (var feature in features)
            {
                if (feature is not JsonObject featureObj)
                    throw new Exception("Fail to read.");

                result.Add(ParseJsonToGisObject(featureObj, layer, geometryType));
            }

            return (IReadOnlyList<IGisObject>)result;
        }
    }
}