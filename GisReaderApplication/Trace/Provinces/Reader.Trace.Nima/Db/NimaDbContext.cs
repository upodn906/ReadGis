using Microsoft.EntityFrameworkCore;
using Reader.Trace.Nima.Db.Entities;

namespace Reader.Trace.Nima.Db
{
    public class NimaDbContext : DbContext
    {
        public NimaDbContext(DbContextOptions<NimaDbContext> opt) : base(opt)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    //Gilan //optionsBuilder.UseSqlServer("Persist Security Info=True;User ID=sa;Initial Catalog=PmGilanProvince;Data Source=10.196.129.118;TrustServerCertificate=True;Password=11236++f6");
        //    optionsBuilder.UseSqlServer("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Pm;Data Source=.;TrustServerCertificate=True");
        //}
        public DbSet<FeederEntity> Feeders { get; set; }
        public DbSet<TraceEntity> Traces { get; set; }
        public DbSet<TraceRecordEntity> TraceRecords { get; set; }
        public DbSet<SymbolEntity> Symbols { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeederEntity>().Property(Q=>Q.FeederId).ValueGeneratedNever();
            modelBuilder.Entity<FeederEntity>().HasKey(Q => Q.FeederId);
            modelBuilder.Entity<FeederEntity>().ToTable( "t_Feeder" , "Edsab");


            modelBuilder.Entity<TraceEntity>().HasKey(Q => Q.TraceId);
            modelBuilder.Entity<TraceEntity>().HasOne(Q => Q.Feeder)
                .WithMany().HasForeignKey(Q => Q.FeederId);
            modelBuilder.Entity<TraceEntity>().ToTable("t_Trace" , "Edsab");


            modelBuilder.Entity<SymbolEntity>().HasKey(Q => Q.SymbolId);
            modelBuilder.Entity<SymbolEntity>().Property(Q => Q.FNAME).HasMaxLength(256);
            modelBuilder.Entity<SymbolEntity>().Property(Q => Q.ENAME).HasMaxLength(256);
            modelBuilder.Entity<SymbolEntity>().ToTable("t_Symbol" , "Edsab");



            modelBuilder.Entity<TraceRecordEntity>().Property(Q=>Q.FeatureClassName)
                .HasMaxLength(64);
            modelBuilder.Entity<TraceRecordEntity>().HasKey(Q => Q.TraceRecordId);
            modelBuilder.Entity<TraceRecordEntity>().HasOne(Q => Q.Feeder)
                .WithMany().HasForeignKey(Q => Q.FeederId);
            modelBuilder.Entity<TraceRecordEntity>().ToTable("t_TraceRecord", "Edsab");
            base.OnModelCreating(modelBuilder);
        }
    }
}
