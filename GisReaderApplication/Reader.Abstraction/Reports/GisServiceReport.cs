using Reader.Abstraction.Reports.Models;

namespace Reader.Abstraction.Reports
{
    public record GisServiceReport : IGisServiceReport
    {
        private Dictionary<int , GisLayerStats> _layers = new();
        private List<GisServiceEvent> _events = new();
        private List<GisServiceError> _errors = new();
        public IReadOnlyDictionary<int, GisLayerStats> Layers => _layers;
        public GisServiceStats Stats { get; private set; } = new GisServiceStats();
        public IReadOnlyList<GisServiceEvent> Events
        {
            get
            {
                lock (_events)
                {
                    return _events.AsReadOnly();
                }
            }
        }
        public IReadOnlyList<GisServiceError> Errors
        {
            get
            {
                lock (_errors)
                {
                    return _errors.AsReadOnly();
                }
            }
        }
        public void AddEvent(GisServiceEvent ev)
        {
            const int limit = 30;
            lock (_events)
            {
                if (_events.Count >= limit)
                {
                    _events.RemoveAt(0);
                }
                _events.Add(ev);
            }
        }
        public void AddError(GisServiceError er)
        {
            const int limit = 30;
            lock (_errors)
            {
                if (_errors.Count >= limit)
                {
                    _errors.RemoveAt(0);
                }
                _errors.Add(er);
            }

            Stats.IncreaseErrorCount();
        }

        public void Initialize()
        {
            _layers = new Dictionary<int, GisLayerStats>();
            _events = new List<GisServiceEvent>();
            _errors = new List<GisServiceError>();
            Stats = new GisServiceStats();
            Stats.Initialize();
        }
        public void InitializeLayers(IEnumerable<GisLayerStats> layers)
        {
            _layers = layers.ToDictionary(Q => Q.Code);
            Stats.TotalLayers = _layers.Count;
        }

        public void InitializeLayerCount(int layer, int count)
        {
            Layers[layer].Initialize(count);
            Stats.IncreaseTotalObjects(count);
        }

        public void Scanned(int layerCode, int count , TimeSpan responseTime)
        {
            Stats.IncreaseScannedObjects(count);
            var layer = Layers[layerCode];
            layer.Scanned(count , responseTime);
            if (layer.ScannedObjects == layer.TotalObjects)
            {
                Finished(layerCode);
            }
        }

        public void Finished(int layer)
        {
            Stats.IncreaseScannedLayer();
            Layers[layer].Finished();
        }

        public void Completed()
        {
            Stats.Finish();
        }
    }
}
