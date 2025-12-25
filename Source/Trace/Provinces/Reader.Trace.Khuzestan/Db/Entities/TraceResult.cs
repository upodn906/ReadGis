using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Khuzestan.Db.Entities
{
    public class TraceResult
    {
        public int Id { get; set; }
        public int FeederObjectId { get; set; }
        public string Json { get; set; }
    }
}
