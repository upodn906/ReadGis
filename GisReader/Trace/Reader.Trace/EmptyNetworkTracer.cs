using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Reader.Trace
{
    public class EmptyNetworkTracer : INetworkTracer
    {
        private readonly ILogger<EmptyNetworkTracer> _logger;

        public EmptyNetworkTracer(ILogger<EmptyNetworkTracer> logger)
        {
            _logger = logger;
        }
        public Task TraceAsync()
        {
            _logger.LogWarning("Network tracer is not registered. skipping trace.");
            return Task.CompletedTask;
        }
    }
}
