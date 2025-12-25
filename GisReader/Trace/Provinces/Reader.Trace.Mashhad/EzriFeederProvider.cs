using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Reader.Infrastructures.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Mashhad
{
    public class EzriFeederProvider : Ezri.IFeederProvider
    {
        private static readonly WKTReader WLTReader = new WKTReader();
        private readonly GisDbContext _ctx;
        private readonly ILogger<EzriFeederProvider> _logger;
        public const int FeederLayerCode = 47;
        //public const int FeederLayerCode = 19;
        public EzriFeederProvider(GisDbContext ctx ,ILogger<EzriFeederProvider> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }
        public async Task<IReadOnlyList<Ezri.Models.FeederTraceModel>> ProcessFeederAsync()
        {
            _logger.LogInformation("Start loading feeders from layer {FeederLayerCode} for tracing.", FeederLayerCode);
            return await _ctx.Objects.AsNoTracking()
                .Where(Q => Q.LayerCode == FeederLayerCode && Q.ShapeStr != null)
                .Select(Q => new Ezri.Models.FeederTraceModel
                {
                    Geometry = FromWkt(Q.ShapeStr!),
                    Id = Q.ObjectId!.Value!
                })
                .ToListAsync();
        }
        private static Geometry FromWkt(string wkt)
        {
            return WLTReader.Read(wkt);
        }
    }
}
