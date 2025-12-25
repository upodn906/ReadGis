using NetTopologySuite.Geometries;

namespace Reader.Infrastructures.Geometries.Models
{
    public record GeometryResult
    {
        public required Geometry LatLong { get; init; }
        public required Geometry Utm { get; init; }
        public required string Shape { get; init; }
    }
}
