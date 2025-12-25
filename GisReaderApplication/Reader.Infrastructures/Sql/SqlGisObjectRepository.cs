using System.Text.Json;
using _Framework.Infrastructures;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reader.Abstraction.Objects;
using Reader.Abstraction.Objects.Default;
using Reader.Infrastructures.Sql.Entities;
//using EFCore.BulkExtensions;

namespace Reader.Infrastructures.Sql
{
    public class SqlGisObjectRepository : BulkRepository<GisObject>, IGisObjectRepository
    {
        private readonly IDbContextFactory<GisDbContext> _factory;

        public SqlGisObjectRepository(IDbContextFactory<GisDbContext> factory,
            ILogger<BulkRepository<GisObject>> logger) : base(100000, logger)
        {
            _factory = factory;
        }

        protected override async Task PerformDbOperationAsync(List<GisObject> items)
        {
            var sqlObj = items.Select(Q =>
            {
                int? objectId = null;
                if (Q.Data.TryGetValue("OBJECTID", out var id))
                    objectId = int.Parse(id.ToString()!);
                else if (Q.Data.TryGetValue("OID", out id))
                    objectId = int.Parse(id.ToString()!);
                return new SqlGisObject
                {
                    Json = JsonSerializer.Serialize(Q.Data),
                    LayerCode = Q.LayerCode,
                    ShapeLatLngStr = Q.ShapeLatLngStr,
                    ShapeStr = Q.ShapeStr,
                    ObjectId = objectId
                };
            });
            await using var ctx = await _factory.CreateDbContextAsync();
            var batches = sqlObj.Chunk(5000);
            foreach (var batch in batches)
            {

                await ctx.BulkInsertAsync(batch, new BulkConfig
                {
                    SetOutputIdentity = false,
                    BulkCopyTimeout = 10000000
                });
            }

            //foreach (var ch in sqlObj)
            //{
            //    await ctx.BulkInsertAsync(ch, new BulkConfig
            //    {
            //        SetOutputIdentity = false
            //    });
            //}
        }

        public async Task CleanAsync()
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            if(ctx.Database.IsSqlServer())
                await ctx.TruncateAsync<SqlGisObject>();
            else
               await ctx.Objects.ExecuteDeleteAsync();
        }
        public async Task AddRangeAsync(IReadOnlyList<IGisObject> iteWms)
        {
            var convert = (IReadOnlyList<GisObject>)iteWms;
            await base.AddRangeAsync(convert);
        }
    }
}
