using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.Data
{
    [ExcludeFromCodeCoverage]
    public class LinkDbContext : DbContext
    {
        public LinkDbContext(DbContextOptions<LinkDbContext> options) : base(options)
        {
        }

        public DbSet<Link> Links => Set<Link>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Link>().Property(l => l.SearchableUrl).HasComputedColumnSql(@"upper(regexp_replace(""LinkUrl"", '^https?:\/\/', ''))", stored: true);
        }

        public void ClearTracked()
        {
            ChangeTracker.Clear();
        }
    }
}
