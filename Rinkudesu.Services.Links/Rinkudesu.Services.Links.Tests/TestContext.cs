using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Data;

namespace Rinkudesu.Services.Links.Tests
{
    public class TestContext : IDisposable
    {
        private readonly LinkDbContext context;
        private readonly SqliteConnection connection;

        public LinkDbContext DbContext => context;

        private TestContext()
        {

        }

        private TestContext(LinkDbContext context, SqliteConnection connection)
        {
            this.connection = connection;
            this.context = context;
        }

        public static TestContext GetTestContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<LinkDbContext>().UseSqlite(connection);
            var dbContext = new LinkDbContext(options.Options);
            dbContext.Database.EnsureCreated();
            return new TestContext(dbContext, connection);
        }

        public void Dispose()
        {
            context?.Dispose();
            connection?.Close();
            connection?.Dispose();
        }
    }
}