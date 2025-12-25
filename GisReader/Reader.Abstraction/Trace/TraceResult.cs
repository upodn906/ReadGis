namespace Reader.Abstraction.Trace
{
    public class TraceResult
    {
        public int Id { get; set; }
        public int? LayerCode { get; set; }
        public string? LayerName { get; set; }
        public int? FeederId { get; set; }
        public int? FeederObjectId { get; set; }
        public string? FeederGlobalId { get; set; }
        public int? ObjectId { get; set; }
        public string? GlobalId { get; set; }
    }
}
