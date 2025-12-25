namespace Reader.Trace.Nima.Models.Results.Traces
{
    public record FeederTraceModel
    {
        public List<EdgeModel> Edges { get; set; } = new();
        public List<JunctionModel> Junctions { get; set; } = new();
        public List<PolygonModel> Polygons { get; set; } = new();

    }
}
