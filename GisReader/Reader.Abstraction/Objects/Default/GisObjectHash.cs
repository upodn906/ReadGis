using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Abstraction.Objects.Default
{
    public record GisObjectHash
    {
        public required int Id { get; set; }
        public required string Hash { get; set; }
    }
}
