namespace Reader.Trace.Nima.Db.Entities;

public class SymbolEntity
{
    public int SymbolId { get; set; }
    public string ENAME { get; set; }
    public string FNAME { get; set; }
    public int FTYPE { get; set; }
    public int WebId { get; set; }
    public int ClassId { get; set; }
    public int Status { get; set; }
    public int VoltazheLevel { get; set; }
    public int MinScale { get; set; }
    public int MaxScale { get; set; }
}