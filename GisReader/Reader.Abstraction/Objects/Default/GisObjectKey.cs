namespace Reader.Abstraction.Objects.Default
{
    public record GisObjectKey
    {
        public required int ObjectId { get; set; }
        public required int LayerCode { get; set; }
    }
}
