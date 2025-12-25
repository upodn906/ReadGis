using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.FaraOmranNegar
{
    public class FaraOmranNegarNetworkTracerConfig
    {
        public int FeederLayerCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TraceUrl { get; set; }
        public string TokenUrl { get; set; }
    }
}
