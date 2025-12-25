using Microsoft.EntityFrameworkCore;
using Reader.Infrastructures.Sql.Entities;

namespace Reader.Infrastructures.Sql
{
    public class GisDbContext : DbContext
    {
        public GisDbContext(DbContextOptions<GisDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SqlGisObject>()
                .ToTable("t_GisInfoTmp", "Gis");

            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.Id).HasColumnName("GisInfoTmpId");
            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.LayerCode)
                .HasColumnName("GisSymbolCode");
            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.Json)
                .HasColumnName("GisInfoJson");
            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.ShapeStr)
                .HasColumnName("GisInfoShapeStr");
            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.ShapeLatLngStr)
                .HasColumnName("GisInfoShapeLatLngStr");
            modelBuilder.Entity<SqlGisObject>()
                .Property(Q => Q.ObjectId)
                .HasColumnName("GisInfoGeoCode");



            modelBuilder.Entity<SqlGisLayer>()
                .ToTable("t_GisInfoTmpSymbol", "Gis");

            modelBuilder.Entity<SqlGisLayer>()
                .Property(Q=>Q.Name)
                .HasMaxLength(120)
                .HasColumnName("GisSymbolName")
                .IsRequired();

            modelBuilder.Entity<SqlGisLayer>()
                .Property(Q => Q.Code)
                .HasColumnName("GisSymbolCode")
                .IsRequired();

            modelBuilder.Entity<SqlGisLayer>()
                .Property(Q => Q.Id)
                .HasColumnName("GisInfoTmpSymbolId")
                .IsRequired();
        }
        public DbSet<SqlGisObject> Objects { get; set; }
        public DbSet<SqlGisLayer> Layers { get; set; }
    }
}
