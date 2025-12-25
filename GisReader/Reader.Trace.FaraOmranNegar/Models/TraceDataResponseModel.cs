using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.FaraOmranNegar.Models
{
    public class TraceDataResponseModel
    {
        [JsonPropertyName("GISTableCode")]
        public string GISTableCode { get; set; }

        [JsonPropertyName("length")]
        public double Length { get; set; }

        [JsonPropertyName("GISCodes")]
        public List<string> GISCodes { get; set; }
    }
}
