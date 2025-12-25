using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.Ezri.Models
{

    public class TraceResponse
    {
        [JsonPropertyName("totalCost")]
        public int TotalCost { get; set; }

        [JsonPropertyName("edges")]
        public List<Edge> Edges { get; set; }

        [JsonPropertyName("junctions")]
        public List<Junction> Junctions { get; set; }

        [JsonPropertyName("flagsNotFound")]
        public List<object> FlagsNotFound { get; set; }

        [JsonPropertyName("barriersNotFound")]
        public List<object> BarriersNotFound { get; set; }
    }


}
