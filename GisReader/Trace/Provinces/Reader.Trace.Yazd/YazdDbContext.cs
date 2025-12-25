using Microsoft.EntityFrameworkCore;
using Reader.Infrastructures.Sql;
using Reader.Trace.Yazd.Models;

namespace Reader.Trace.Yazd
{
    public class YazdDbContext : GisDbContext
    {
        public YazdDbContext(DbContextOptions<GisDbContext> options) : base(options)
        {
        }
        public DbSet<Feeder> Feeders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Feeder>().Property(Q => Q.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Feeder>()
                .ToTable("t_Feeder", "Gis");
            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.Id)
                .HasColumnName("FeederId");
            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.Name)
                .HasMaxLength(256)
                .HasColumnName("FeederName");

            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.FiderCode)
                .HasColumnName("FeederCode");

            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.PostName)
                .HasMaxLength(256)
                .HasColumnName("FeederPostName");


            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.X)
                .HasMaxLength(32)
                .HasColumnName("FeederX");

            modelBuilder.Entity<Feeder>()
                .Property(Q => Q.Y)
                .HasMaxLength(32)
                .HasColumnName("FeederY");
        }
    }
}
