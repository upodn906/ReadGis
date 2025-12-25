using System.Text.Json.Nodes;
using Reader.Infrastructures.Geometries.Models;

namespace Reader.Infrastructures.Geometries
{
    public interface IGisGeometryFactory
    {
        GeometryResult? Create(string geometryType, JsonNode node);
    }
}
