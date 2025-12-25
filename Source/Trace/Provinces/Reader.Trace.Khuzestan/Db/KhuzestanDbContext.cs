using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reader.Trace.Khuzestan.Db.Entities;

namespace Reader.Trace.Khuzestan.Db
{
    public class KhuzestanDbContext : DbContext
    {
        public KhuzestanDbContext(DbContextOptions<KhuzestanDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TraceResult>()
                .ToTable("t_RawNetworkTrace", "Gis");

            modelBuilder.Entity<TraceResult>().Property(Q => Q.Id)
                .HasColumnName("RawNetworkTraceId");

            modelBuilder.Entity<TraceResult>().Property(Q => Q.Json)
                .HasColumnName("RawNetworkTraceJson");

            modelBuilder.Entity<TraceResult>().Property(Q => Q.FeederObjectId)
                .HasColumnName("FeederGeoCode");
        }
        public DbSet<TraceResult> TraceResults { get; set; }
    }
}
