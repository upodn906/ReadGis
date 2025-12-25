using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Reader.Infrastructures.Geometries.Models;

namespace Reader.Infrastructures.Geometries;

public class WfsGeometryFactory : IGisGeometryFactory
{
    public static readonly GeometryFactory GeoFactory = new();
    private readonly ILogger<WfsGeometryFactory> _logger;

    public WfsGeometryFactory(ILogger<WfsGeometryFactory> logger)
    {
        _logger = logger;
    }
    public GeometryResult? Create(string geometryType, JsonNode node)
    {
        try
        {
            var shape = geometryType.ToUpper();
            switch (shape)
            {
                case "POINT":
                    var pointCoordinate = GetSingleCoordinate(node);
                    if (pointCoordinate == null)
                        return null;
                    var latLngPoint = GeoFactory.CreatePoint(pointCoordinate);
                    var utmPoint = GeoFactory.CreatePoint(GetUtmCoordinate(pointCoordinate)[0]);
                    return new GeometryResult()
                    {
                        Shape = "POINT",
                        LatLong = latLngPoint,
                        Utm = utmPoint
                    };
                case "LINESTRING":
                    var lineCoordinates = GetLineStringCoordinate(node);
                    if (lineCoordinates == null)
                        return null;
                    var latLngLineString = GeoFactory.CreateLineString(lineCoordinates);
                    var utmLineString = GeoFactory.CreateLineString(GetUtmCoordinate(lineCoordinates));
                    return new GeometryResult()
                    {
                        Shape = "LINESTRING",
                        LatLong = latLngLineString,
                        Utm = utmLineString
                    };
                case "MULTILINESTRING":
                    var multiLineCoordinates = GetMultiLineStringCoordinate(node);
                    if (multiLineCoordinates == null)
                        return null;
                    var latLngLineStrings = multiLineCoordinates.Select(Q =>
                        GeoFactory.CreateLineString(Q)).ToArray();
                    var latLngMultiLineString = GeoFactory.CreateMultiLineString(latLngLineStrings);
                    var utmLineStrings = multiLineCoordinates.Select(Q => GetUtmCoordinate(Q))
                        .Select(Q => GeoFactory.CreateLineString(Q)).ToArray();
                    var utmMultiLineString = GeoFactory.CreateMultiLineString(utmLineStrings);
                    return new GeometryResult()
                    {
                        Shape = "MULTILINESTRING",
                        LatLong = latLngMultiLineString,
                        Utm = utmMultiLineString
                    };
                case "MULTIPOLYGON":
                    var muliPolygonLatLngCoordinates = GetMultiPolygonCoordinate(node);
                    if (muliPolygonLatLngCoordinates == null)
                        return null;
                    var multiLatLngPolygons = muliPolygonLatLngCoordinates.Select(Q => GeoFactory.CreatePolygon(Q))
                        .ToArray();
                    var latLngMultiPolygon = GeoFactory.CreateMultiPolygon(multiLatLngPolygons);
                    var polygonUtmCoordinates = muliPolygonLatLngCoordinates.Select(Q => GetUtmCoordinate(Q)).ToArray();
                    var urmPolygons = polygonUtmCoordinates.Select(Q => GeoFactory.CreatePolygon(Q))
                        .ToArray();
                    var utmMultiPolygon = GeoFactory.CreateMultiPolygon(urmPolygons);
                    return new GeometryResult()
                    {
                        Shape = "MULTIPOLYGON",
                        LatLong = latLngMultiPolygon,
                        Utm = utmMultiPolygon
                    };
                case "POLYGON":
                    var polygonCoordinates = GetPolygonCoordinate(node);
                    if (polygonCoordinates == null)
                        return null;
                    var latLngPolygon = GeoFactory.CreatePolygon(polygonCoordinates);
                    var utmCoordinates = polygonCoordinates.Select(Q => GetUtmCoordinate(Q)).ToArray();
                    var utmPolygon = GeoFactory.CreatePolygon(GetUtmCoordinate(polygonCoordinates));
                    return new GeometryResult()
                    {
                        Shape = "POLYGON",
                        LatLong = latLngPolygon,
                        Utm = utmPolygon
                    };
                default:
                    throw new InvalidDataException($"unable to create geometry for {shape}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fail to map geometry.");
            return null;
        }
    }

    private Coordinate? GetSingleCoordinate(JsonNode node)
    {
        if (node["coordinates"] is JsonArray array == false || array.Count < 2)
            return null;

        return new Coordinate
        {
            X = array[0]!.GetValue<double>(),
            Y = array[1]!.GetValue<double>(),
        };
    }

    private List<Coordinate[]>? GetMultiLineStringCoordinate(JsonNode node)
    {
        if (node["coordinates"] is JsonArray array == false)
        {
            _logger.LogWarning("MultiLineString is not an array | {json}." , node.ToJsonString());
            return null;
        }
        if (array.Count < 1)
        {
            _logger.LogWarning("MultiLineString contains less than 1 elements | {json}.", node.ToJsonString());
            return null;
        }
        var result = new List<Coordinate[]>();
        for (var i = 0; i < array.Count; i++)
        {
            var line = array[i]!.AsArray();
            if (line.Count < 1)
            {
                _logger.LogWarning("MultiLineString inner line contains less than 1 elements | {json}.", node.ToJsonString());
                continue;
            }
            var crd = new Coordinate[line.Count];
            result.Add(crd);
            for (var j = 0; j < line.Count; j++)
            {
                crd[j] = new Coordinate
                {
                    X = line[j]![0]!.GetValue<double>(),
                    Y = line[j]![1]!.GetValue<double>(),
                };
            }
        }

        return result;
    }

    private Coordinate[]? GetLineStringCoordinate(JsonNode node)
    {
        if (node["coordinates"] is JsonArray array == false || array.Count < 2)
            return null;
        var result = new Coordinate[array.Count];
        for (var i = 0; i < array.Count; i++)
        {
            result[i] = new Coordinate
            {
                X = array[i]![0]!.GetValue<double>(),
                Y = array[i]![1]!.GetValue<double>(),
            };
        }

        return result;
    }
    private Coordinate[]? GetPolygonCoordinate(JsonNode node)
    {
        if (node["coordinates"]?[0] is JsonArray array == false || array.Count < 2)
            return null;
        var result = new Coordinate[array.Count];
        for (var i = 0; i < array.Count; i++)
        {
            result[i] = new Coordinate
            {
                X = array[i]![0]!.GetValue<double>(),
                Y = array[i]![1]!.GetValue<double>(),
            };
        }

        return result;
    }
    private Coordinate[][]? GetMultiPolygonCoordinate(JsonNode node)
    {
        if (node["coordinates"]?[0] is JsonArray array == false || array.Count < 1)
            return null;
        var result = new Coordinate[array.Count][];
        for (var i = 0; i < array.Count; i++)
        {
            var polygon = array[0]?.AsArray();
            if (polygon == null)
                continue;
            result[i] = new Coordinate[polygon.Count];
            for (var j = 0; j < polygon.Count; j++)
            {
                result[i][j] = new Coordinate
                {
                    X = polygon[j]![0]!.GetValue<double>(),
                    Y = polygon[j]![1]!.GetValue<double>(),
                };
            }
        }

        return result;
    }

    private static Coordinate[] GetUtmCoordinate(params Coordinate[] coordinates)
    {
        var result = new Coordinate[coordinates.Length];
        for (var i = 0; i < coordinates.Length; i++)
        {
            var item = coordinates[i];
            var zone = GetZone(item.Y, item.X);
            var ctfac = new CoordinateTransformationFactory();
            var wgs84 = GeographicCoordinateSystem.WGS84;
            var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, item.Y > 0);
            var trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);
            var utmCoordinate = trans.MathTransform.Transform(new[] { item.X, item.Y });
            result[i] = (new Coordinate
            {
                X = utmCoordinate[0],
                Y = utmCoordinate[1],
            });
        }

        return result;
    }

    static int GetZone(double latitude, double longitude)
    {
        if (latitude >= 56 && latitude < 64 && longitude >= 3 && longitude < 12)
            return 32;

        if (latitude >= 72 && latitude < 84)
        {
            if (longitude >= 0 && longitude < 9)
                return 31;
            if (longitude >= 9 && longitude < 21)
                return 33;
            if (longitude >= 21 && longitude < 33)
                return 35;
            if (longitude >= 33 && longitude < 42)
                return 37;
        }

        var zone = (int)Math.Floor((longitude + 180) / 6) + 1;
        if (latitude < 0)
            zone = -zone;

        return zone;
    }
}