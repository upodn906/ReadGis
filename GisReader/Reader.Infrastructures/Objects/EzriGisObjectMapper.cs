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
    public class EzriGisObjectMapper<TGisObject> : GisObjectMapper<TGisObject>, IGisObjectMapper where TGisObject : IGisObject , new()
    {
        private readonly IGisObjectTransformer _transformer;
        private readonly IGisGeometryFactory _geometryFactory;
        private readonly IGisObjectProcessor _processor;
        private readonly ILogger<EzriGisObjectMapper<TGisObject>> _logger;

        public EzriGisObjectMapper(
            IGisObjectTransformer transformer,
            IGisGeometryFactory geometryFactory ,
            IGisObjectProcessor processor,
            ILogger<EzriGisObjectMapper<TGisObject>> logger) :base(geometryFactory)
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
                {
                    info.Data.TryAdd(_transformer.GetFieldName(key), flatValue);
                    if (key == "OBJETCID" && flatValue.GetType() == typeof(int))
                    {
                        //info.ObjectId = (int)flatValue;
                    }
                }
            }
            _processor.ProcessAsync(info);
            if (obj?["geometry"] is JsonObject geometry)
            {
                MapGeometry(info, geometry, geometryType);
            }
            else
            {
                _logger.LogWarning("Object doesnt have geometry.");
            }
            //info.Hash = GenerateHash(info);
            return info;
        }
        private static string GetNameLastSegment(in string key)
        {
            var split = key.Split('.');
            if (split.Length <= 1)
                return key.ToUpper();
            return split[^1].ToUpper();
        }
        public override IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer)
        {
            if (obj["features"] is not JsonArray features)
                throw new Exception($"obj does not contain features.");

            var geometryType = obj["geometryType"]?.ToString();


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