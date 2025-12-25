namespace Reader.Trace.Nima.Models.Results
{
    public record FeederModel
    {
        public int Id { get; set; }
        public string FName { get; set; }
        public int ObjectId { get; set; }
        public int FeederType { get; set; }
    }
}
