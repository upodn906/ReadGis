namespace Reader.Abstraction.Clients.Models.Requests
{
    public class RequestGisObjects
    {
        public string LayerName { get; set; }
        public string FieldName { get; set; }
        public string StartId { get; set; }
        public string StopId { get; set; }
    }
}
