using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Framework.Entities
{
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }
}
