namespace Reader.Abstraction.Services.Models
{
    public record ScanOptions
    {
        public int Threads { get; set; }
        public int ReadBatchSize { get; set; }
    }
}
