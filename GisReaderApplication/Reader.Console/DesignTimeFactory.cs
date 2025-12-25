using Microsoft.EntityFrameworkCore.Design;
using Reader.Infrastructures.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reader.Trace.Khuzestan.Db;
using Reader.Trace.Nima.Db;

namespace Reader.Console
{
    public class DesignTimeFactory :
        IDesignTimeDbContextFactory<GisDbContext> ,
        IDesignTimeDbContextFactory<KhuzestanDbContext> ,
        IDesignTimeDbContextFactory<NimaDbContext>
    {
        public GisDbContext CreateDbContext(string[] args)
        {
            //return new GisDbContext(new DbContextOptionsBuilder<GisDbContext>()
            //    .UseSqlite("Data Source=Database.db").Options);
            return new GisDbContext(new DbContextOptionsBuilder<GisDbContext>()
                .UseSqlServer().Options);
        }

        KhuzestanDbContext IDesignTimeDbContextFactory<KhuzestanDbContext>.CreateDbContext(string[] args)
        {
            return new KhuzestanDbContext(new DbContextOptionsBuilder<KhuzestanDbContext>()
                .UseSqlServer().Options);
        }

        NimaDbContext IDesignTimeDbContextFactory<NimaDbContext>.CreateDbContext(string[] args)
        {
            return new NimaDbContext(new DbContextOptionsBuilder<NimaDbContext>()
                .UseSqlServer().Options);
        }
    }
}
