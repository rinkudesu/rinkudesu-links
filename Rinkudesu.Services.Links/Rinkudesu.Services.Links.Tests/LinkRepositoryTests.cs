using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.QueryModels;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Xunit;

namespace Rinkudesu.Services.Links.Tests
{
    public class LinkRepositoryTests : ContextTests
    {
        private List<Link> links = new List<Link>();
        private async Task PopulateLinksAsync()
        {
            links = new List<Link>
            {
                new Link(),
                new Link { LinkUrl = "http://localhost/" },
                new Link { Title = "ayaya" },
                new Link { Description = "tuturu*" },
                new Link { PrivacyOptions = Link.LinkPrivacyOptions.Public },
                new Link { CreatingUserId = "a" }
            };
            _context.Links.AddRange(links);
            await _context.SaveChangesAsync();
        }

        private LinkRepository CreateRepository()
        {
            return new LinkRepository(_context, new NullLogger<LinkRepository>());
        }

        [Fact]
        public async Task LinkRepositoryGetAllLinks_queryModelWithDefaultValues_OnlyPublicReturned()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();

            var results = await repo.GetAllLinksAsync(new LinkListQueryModel());
            var returnedLinks = results.ToList();

            Assert.Equal(links.Count(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public), returnedLinks.Count);
        }

        [Fact]
        public async Task LinkRepositoryGetLink_RequestedPublicLink_LinkReturned()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var requestGuid = links.First(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public).Id;

            var result = await repo.GetLinkAsync(requestGuid);

            Assert.Equal(requestGuid, result.Id);
        }

        [Fact]
        public async Task LinkRepositoryGetLink_RequestedPrivateLinkWithUserId_LinkReturned()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var requestGuid = links.First(l => l.CreatingUserId == "a").Id;

            var result = await repo.GetLinkAsync(requestGuid, "a");

            Assert.Equal(requestGuid, result.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("aa")]
        [InlineData("")]
        [InlineData("aosujdfhao")]
        public async Task LinkRepositoryGetLink_RequestedPrivateLinkWithInvalidUserId_DataNotFoundThrown(string? userId)
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var requestGuid = links.First(l => l.CreatingUserId == "a").Id;

            await Assert.ThrowsAsync<DataNotfoundException>(() => repo.GetLinkAsync(requestGuid, userId));
        }

        [Fact]
        public async Task LinkRepositoryCreateLink_NewLink_LinkAddedCorrectly()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = new Link();

            await repo.CreateLinkAsync(link);

            Assert.Contains(_context.Links, l => l.Id == link.Id);
        }

        [Fact]
        public async Task LinkRepositoryCreateLink_NewLinkWithDuplicateId_DataAlreadyExistsThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            _context.ClearTracked();
            var link = new Link { Id = links.First().Id };

            await Assert.ThrowsAsync<DataAlreadyExistsException>(() => repo.CreateLinkAsync(link));
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateExistingLinkWithValidUserId_LinkChanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == "a");
            var updatedLink = new Link
            {
                Id = link.Id,
                Description = "test",
                CreatingUserId = "a" //TODO: remove this assignment as this should not be changeable here
            };
            _context.ClearTracked();

            await repo.UpdateLinkAsync(updatedLink, "a");

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Equal(updatedLink.Description, dbLink.Description);
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateExistingLinkWithInvalidUserId_DataNotFoundThrownAndLinkUnchanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == "a");
            link.Description = "test";
            _context.ClearTracked();

            await Assert.ThrowsAsync<DataNotfoundException>(() => repo.UpdateLinkAsync(link, "b"));

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Null(dbLink.Description);
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateLinkWithInvalidId_DataNotFoundThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = new Link { CreatingUserId = "a" };

            await Assert.ThrowsAsync<DataNotfoundException>(() => repo.UpdateLinkAsync(link, "a"));
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteExistingLinkWithValidUserId_LinkDeleted()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == "a");
            _context.ClearTracked();

            await repo.DeleteLinkAsync(link.Id, "a");

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Null(dbLink);
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteExistingLinkWithInvalidUserId_DataNotFoundThrownAndLinkUnchanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == "a");
            link.Description = "test";
            _context.ClearTracked();

            await Assert.ThrowsAsync<DataNotfoundException>(() => repo.DeleteLinkAsync(link.Id, "b"));

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.NotNull(dbLink);
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteLinkWithInvalidId_DataNotFoundThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = new Link { CreatingUserId = "a" };

            await Assert.ThrowsAsync<DataNotfoundException>(() => repo.DeleteLinkAsync(link.Id, "a"));
        }
    }
}