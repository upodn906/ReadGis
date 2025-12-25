using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.Mazandaran.Models
{
    public class TraceEdgeResponseModel
    {
        [JsonPropertyName("sourceLayer")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("gisCode")]
        public string GISCodes { get; set; }
    }
}
