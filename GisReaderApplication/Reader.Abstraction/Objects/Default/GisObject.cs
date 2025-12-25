namespace Reader.Abstraction.Objects.Default;

public class GisObject : IGisObject
{
    public int LayerCode { get; set; }
    public string? LayerName { get; set; }
    public string? LayerStandardName { get; set; }
    public int CompanyId { get; set; }
    public string? ShapeStr { get; set; }
    public string? ShapeLatLngStr { get; set; }

    //public Dictionary<string, object>? MetaData { get; set; }

    public Dictionary<string, object> Data { get; set; }
    = new Dictionary<string, object>();
}