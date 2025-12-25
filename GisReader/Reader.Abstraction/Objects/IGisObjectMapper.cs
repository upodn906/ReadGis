using System.Text.Json.Nodes;
using Reader.Abstraction.Layers.Models;
using Reader.Abstraction.Objects.Default;

namespace Reader.Abstraction.Objects;

public interface IGisObjectMapper
{
    IReadOnlyList<IGisObject> Map(JsonObject obj, GisLayer layer);
}
