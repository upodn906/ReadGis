using System.Linq;
using System.Text.Json.Nodes;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Reader.Infrastructures.Geometries.Models;

namespace Reader.Infrastructures.Geometries
{
    public class EzriGeometryFactory : IGisGeometryFactory
    {
        public static readonly GeometryFactory GeoFactory = new();

        public GeometryResult? Create(string? geometryType, JsonNode node)
        {
            try
            {
                var normalized = geometryType?.Trim().ToUpper();
                switch (normalized)
                {
                    case "ESRIGEOMETRYPOINT":
                        return new GeometryResult
                        {
                            LatLong = GeoFactory.CreatePoint(GetLatLongCoordinate(node)),
                            Utm = GeoFactory.CreatePoint(GetUtmCoordinate(node)),
                            Shape = "Point"
                        };
                    case "ESRIGEOMETRYPOLYGON" when node["rings"] is JsonArray array:
                        if (array.Count == 0)
                            return null;
                        return new GeometryResult
                        {
                            LatLong = GeoFactory.CreatePolygon(GetLatLongCoordinates(array)),
                            Utm = GeoFactory.CreatePolygon(GetUtmCoordinates(array)),
                            Shape = "Polygon"
                        };
                    case "ESRIGEOMETRYPOLYLINE" when node["paths"] is JsonArray array:
                        return new GeometryResult
                        {
                            LatLong = GeoFactory.CreateLineString(GetLatLongCoordinates(array)),
                            Utm = GeoFactory.CreateLineString(GetUtmCoordinates(array)),
                            Shape = "LineString"
                        };
                }

                throw new InvalidDataException($"unable to create geometry for {normalized}.");
            }
            catch
            {
                //Add if for NaN
                return null;
            }
        }

        private Coordinate GetUtmCoordinate(JsonNode node)
        {
            return GetUtmCoordinate(GetOrThrow(node, "x"), GetOrThrow(node, "y"));
        }

        private Coordinate[] GetUtmCoordinates(JsonArray array)
        {
            if (array[0] is JsonArray innerArray)
            {
                array = innerArray;
            }

            return array.Select(Q =>
                GetUtmCoordinate(GetOrThrow(Q, 0), GetOrThrow(Q, 1))).ToArray();
        }

        private Coordinate GetLatLongCoordinate(JsonNode node)
        {
            return new Coordinate(
                GetOrThrow(node, "x").GetValue<double>(),
                GetOrThrow(node, "y").GetValue<double>());
        }

        private Coordinate[] GetLatLongCoordinates(JsonArray array)
        {
            if (array[0] is JsonArray innerArray)
            {
                array = innerArray;
            }

            return array.Select(Q => new Coordinate(
                GetOrThrow(Q, 0).GetValue<double>(),
                GetOrThrow(Q, 1).GetValue<double>())).ToArray();
        }

        private JsonNode GetOrThrow(JsonNode node, in string key)
        {
            return node[key] ?? throw new Exception($"Fail read {key} from node {node.ToJsonString()}");
        }

        private JsonNode GetOrThrow(JsonNode node, int index)
        {
            return node[index] ?? throw new Exception($"Fail read index {index} from node {node.ToJsonString()}");
        }

        private static Coordinate GetUtmCoordinate(JsonNode x, JsonNode y)
        {
            var latitude = y.GetValue<double>();
            var longitude = x.GetValue<double>();
            var zone = GetZone(latitude, longitude);
            var ctfac = new CoordinateTransformationFactory();
            var wgs84 = GeographicCoordinateSystem.WGS84;
            var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, latitude > 0);
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);
            var result = trans.MathTransform.Transform(new[] { longitude, latitude });
            return new Coordinate
            {
                X = result[0],
                Y = result[1],
            };
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
}