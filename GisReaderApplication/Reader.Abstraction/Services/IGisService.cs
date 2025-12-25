using _Framework.Service._Common;
using Reader.Abstraction.Services.Models;

namespace Reader.Abstraction.Services
{
    public interface IGisService
    {
        GisServiceConfiguration Configuration { get; }
        ServiceStatus Status { get; }
        Task<ServiceResult> ScanAllLayersObjectsAsync(ScanOptions options);
    }
}
