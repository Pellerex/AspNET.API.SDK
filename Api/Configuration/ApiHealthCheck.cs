using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace Ecosystem.ML.LanguageProcessingApi.Configuration
{
    public class ApiHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var result = HealthCheckResult.Healthy();
            return Task.FromResult(result);
        }
    }
}