using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.Data
{
    [ExcludeFromCodeCoverage]
    public class LinkDbContext : DbContext
    {
#if DEBUG
        public LinkDbContext()
        {
        }
#endif

        public LinkDbContext(DbContextOptions<LinkDbContext> options) : base(options)
        {
        }

        public DbSet<Link> Links => Set<Link>();

        public void ClearTracked()
        {
            ChangeTracker.Clear();
        }
    }
}