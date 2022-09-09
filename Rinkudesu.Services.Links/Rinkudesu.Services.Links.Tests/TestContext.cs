using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Data;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Tests
{
    public class TestContext : IDisposable
    {
        private readonly LinkDbContext context;

        public LinkDbContext DbContext => context;

        private TestContext(LinkDbContext context)
        {
            this.context = context;
        }

        public static TestContext GetTestContext()
        {
            var databaseName = Guid.NewGuid().ToString();
            var connectionString = $"Server=127.0.0.1;Port=5432;Database={databaseName};User Id=postgres;Password=postgres;";
            var options = new DbContextOptionsBuilder<LinkDbContext>().UseNpgsql(
                connectionString, providerOptions => {
                    providerOptions.EnableRetryOnFailure();
                    providerOptions.MigrationsAssembly("Rinkudesu.Services.Links");
                });
            var dbContext = new LinkDbContext(options.Options);
            dbContext.Database.EnsureCreated();
            return new TestContext(dbContext);
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
