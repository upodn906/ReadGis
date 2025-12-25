using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Framework.Service._Common
{
    public abstract record ServiceEvent
    {
        public DateTime DateTime { get; }
        public string Message { get; }
        protected ServiceEvent(string message)
        {
            DateTime = DateTime.Now;
            Message = message;
        }
    }
}
