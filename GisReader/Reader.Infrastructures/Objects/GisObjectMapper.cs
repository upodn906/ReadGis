using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects;
using Reader.Infrastructures.Geometries;

namespace Reader.Infrastructures.Objects;

public abstract class GisObjectMapper<TGisObject> : IGisObjectMapper where TGisObject : IGisObject, new()
{
    private readonly IGisGeometryFactory _geometryFactory;
    protected GisObjectMapper(IGisGeometryFactory gisGeometryFactory)
    {
        _geometryFactory = gisGeometryFactory;
    }
    public abstract IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer);
    public string GenerateHash(TGisObject obj)
    {
        var json = JsonSerializer.Serialize(obj);
        using var md5 = MD5.Create();
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var hashBytes = md5.ComputeHash(jsonBytes);
        return Convert.ToBase64String(hashBytes);
    }
    protected void MapGeometry(TGisObject obj, JsonNode? geometry, in string? geometryType)
    {
        if (geometry == null)
            return;

        if (geometryType == null)
            return;

        var geo = _geometryFactory.Create(geometryType, geometry);
        if (geo == null) return;
        obj.ShapeLatLngStr = geo.LatLong.ToString();
        obj.ShapeStr = geo.Utm.ToString();
    }
    protected object? FlatValue(object? node)
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
