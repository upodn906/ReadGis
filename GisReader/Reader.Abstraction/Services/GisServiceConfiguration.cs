using Reader.Abstraction.Clients;

namespace Reader.Abstraction.Services
{
    public record GisServiceConfiguration
    {
        public GisProvider Provider { get; set; }
        public IReadOnlySet<string> Layers { get; set; }
        public int Threads { get; set; }
        public int BatchSize { get; set; }
        public bool RandomizeBatches { get; set; } = false;
        public bool TruncatedMode { get; set; }
    }
}
