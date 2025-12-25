using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.Mazandaran.Models
{
    public class TraceResponseModel
    {
        //[JsonPropertyName("ResultState")]
        //public bool ResultState { get; set; }

        [JsonPropertyName("result_code")]
        public int ResultCode { get; set; }

        [JsonPropertyName("result_message")]
        public string ResultMessage { get; set; }

        [JsonPropertyName("data")]
        public List<Data> Data { get; set; }

        [JsonPropertyName("edges")]
        public List<Edge> Edge { get; set; }
    }

    public class Data
    {
        //public TraceDataResponseModel data { get; set; }
        [JsonPropertyName("gis_table")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("gis_codes")]
        public List<string> GISCodes { get; set; }
    }

    public class Edge
    {
        //public TraceEdgeResponseModel edge { get; set; }
        [JsonPropertyName("sourceLayer")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("gisCode")]
        public string GISCodes { get; set; }
        public string startGisCode { get; set; }
        public string endGisCode { get; set; }
        public int voltage { get; set; }
    }

}