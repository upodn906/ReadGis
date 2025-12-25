namespace Reader.Trace.Nima.Db.Entities;

public class TraceRecordEntity
{
    public int TraceRecordId { get; set; }
    public int FeederId { get; set; }
    public string FeatureClassName { get; set; }
    public int ObjectId { get; set; }
    public FeederEntity Feeder { get; set; }
}