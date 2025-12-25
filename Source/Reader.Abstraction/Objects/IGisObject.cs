namespace Reader.Abstraction.Objects
{
    public interface IGisObject
    {
        public int LayerCode { get; set; }
        public string? LayerName { get; set; }
        public string? ShapeStr { get; set; }
        public string? ShapeLatLngStr { get; set; }
        public Dictionary<string, object?> Data { get; set; }
    }
}
