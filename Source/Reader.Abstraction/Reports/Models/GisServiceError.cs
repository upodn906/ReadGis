using _Framework.Service._Common;

namespace Reader.Abstraction.Reports.Models;

public record GisServiceError : ServiceError
{
    public GisServiceError(string error) : base(error)
    {
    }
    public static GisServiceError ErrorInReadingAllLayers(string error)
    {
        return new GisServiceError($"خطای [{error}] درخواندن لایه ها رخ داد.");
    }
    public static GisServiceError ErrorInReadingLayerCount(int layerId ,string? layerName, string error)
    {
        return new GisServiceError($"خطای [{error}] در خواندن تعداد لایه [{layerId} | {layerName}] رخ داد.");
    }

    public static GisServiceError ErrorInReadingLayer(string? layer, string error)
    {
        return new GisServiceError($"خطای [{error}] در خواندن لایه [{layer}] رخ داد.");
    }
}