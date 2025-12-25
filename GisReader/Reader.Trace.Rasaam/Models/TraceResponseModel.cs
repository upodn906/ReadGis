using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Reader.Trace.Rasaam.Models
{
    public class TraceResponseModel
    {
        [JsonPropertyName("ResultState")]
        public bool ResultState { get; set; }

        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("ResultMessage")]
        public string ResultMessage { get; set; }

        [JsonPropertyName("Data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<TraceDataResponseModel> data { get; set; }
    }

}