using System;
using Rinkudesu.Services.Links.Data;

namespace Rinkudesu.Services.Links.Tests
{
    public abstract class ContextTests : IDisposable
    {
        private readonly TestContext _testContext;
        protected readonly LinkDbContext _context;

        public ContextTests()
        {
            _testContext = TestContext.GetTestContext();
            _context = _testContext.DbContext;
        }

        public void Dispose()
        {
            _testContext?.Dispose();
        }
    }
}