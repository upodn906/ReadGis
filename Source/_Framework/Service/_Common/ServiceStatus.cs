using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Framework.Service._Common
{
    public enum ServiceStatus
    {
        Idle = 1,
        Running,
        Finished,
        StopDueToError,
        StoppedDueToUserRequest
    }
}
