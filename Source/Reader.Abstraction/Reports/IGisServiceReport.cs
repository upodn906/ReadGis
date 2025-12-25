using Reader.Abstraction.Reports.Models;

namespace Reader.Abstraction.Reports;

public interface IGisServiceReport
{
    IReadOnlyDictionary<int, GisLayerStats> Layers { get; }
    GisServiceStats Stats { get; }
    IReadOnlyList<GisServiceEvent> Events { get; }
    IReadOnlyList<GisServiceError> Errors { get; }
    void AddEvent(GisServiceEvent ev);
    void AddError(GisServiceError er);
    void Initialize();
    void InitializeLayers(IEnumerable<GisLayerStats> layers);
    void InitializeLayerCount(int layer, int count);
    void Scanned(int layerCode, int count , TimeSpan responseTime);
    void Finished(int layer);
    void Completed();
}