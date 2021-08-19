using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.Data
{
    public class LinkDbContext : DbContext
    {
        public LinkDbContext(DbContextOptions<LinkDbContext> options) : base(options)
        {
        }

        public DbSet<Link> Links => Set<Link>();
    }
}