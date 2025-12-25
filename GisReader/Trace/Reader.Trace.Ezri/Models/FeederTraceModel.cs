using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Ezri.Models
{
    public class FeederTraceModel
    {
        public required int Id { get; init; }
        public required NetTopologySuite.Geometries.Geometry Geometry { get; init; }
    }
}
