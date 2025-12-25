using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Framework.Infrastructures
{
    public interface IBulkRepository<in T>
    {
        Task AddRangeAsync(IReadOnlyList<T> iteWms);
        Task FlushAsync();
    }
}
