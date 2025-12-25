using Reader.Abstraction.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reader.Abstraction.Layers.Models;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;
using Reader.Infrastructures.Sql.Entities;

namespace Reader.Infrastructures.Sql
{
    public class SqlGisLayerRepository : IGisLayerRepository
    {
        private readonly IDbContextFactory<GisDbContext> _factory;

        public SqlGisLayerRepository(IDbContextFactory<GisDbContext> factory)
        {
            _factory = factory;
        }
        public async Task UpdateAllAsync(IEnumerable<GisLayer> layers)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            await ctx.Layers.ExecuteDeleteAsync();
            ctx.Layers.AddRange(layers.Select(Q => new SqlGisLayer
            {
                Code = Q.Id,
                Name = Q.EnName,
            }));
            await ctx.SaveChangesAsync();
        }
    }
}
