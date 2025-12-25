using Reader.Abstraction.Layers.Models;

namespace Reader.Abstraction.Layers
{
    public interface IGisLayerProvider
    {
        Task<IReadOnlyList<GisLayer>> GetDefaultLayersAsync();
    }
}
