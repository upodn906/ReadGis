using System.ComponentModel.DataAnnotations;

namespace Reader.Infrastructures.Sql.Entities
{
    public class SqlGisObject
    {
        [Key]
        public int Id { get; set; }
        //public int ObjectId { get; set; }
        //public int LayerId { get; set; }
        //public GisLayer Layer { get; set; }
        //public string? GisInfoShapeStr { get; set; }
        public int LayerCode { get; set; }
        public string? ShapeStr { get; set; }
        //public string? GisInfoShapeLatLngStr { get; set; }
        public string? ShapeLatLngStr { get; set; }
        //public string? MapId { get; set; }
        public string? Json { get; set; }

        public int? ObjectId { get; set; }
        //public string? GisInfoGeoCode { get; set; }
        //public string? GisInfoJson { get; set; }
        //public string? FeederGeoCode { get; set; }
        //public string? GisSymbolCode { get; set; }
        //public int Id { get; set; }
        //public int LayerCode { get; set; }
        //public string? LayerName { get; set; }
        //public string? LayerStandardName { get; set; }
        //public int CompanyId { get; set; }
        //public string? ShapeLatLngStr { get; set; }
        //public string DataJson { get; set; }
    }
}
