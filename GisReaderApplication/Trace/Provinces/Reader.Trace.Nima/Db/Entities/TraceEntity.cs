namespace Reader.Trace.Nima.Db.Entities;

public class TraceEntity
{
    public int TraceId { get; set; }
    public int FeederId { get; set; }
    public string TraceJson { get; set; }
    public FeederEntity Feeder { get; set; }
}