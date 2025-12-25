using Reader.Abstraction.Layers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Abstraction.Layers
{
    public interface IGisLayerRepository
    {
        Task UpdateAllAsync(IEnumerable<GisLayer> layers);
    }
}
