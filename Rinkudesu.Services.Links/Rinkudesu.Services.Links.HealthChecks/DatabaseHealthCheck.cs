using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Rinkudesu.Services.Links.Data;

namespace Rinkudesu.Services.Links.HealthChecks
{
    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly LinkDbContext _context;

        public DatabaseHealthCheck(LinkDbContext context)
        {
            this._context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false))
            {
                return HealthCheckResult.Unhealthy("Unable to connect to the database");
            }
            var migrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
            if (migrations.Any())
            {
                return HealthCheckResult.Degraded("Database migrations are pending, functionality may be limited");
            }
            return HealthCheckResult.Healthy();
        }
    }
}