using Reader.Trace.Ezri.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Trace.Ezri
{
    public interface IFeederProvider
    {
        Task<IReadOnlyList<FeederTraceModel>> ProcessFeederAsync();
    }
}
