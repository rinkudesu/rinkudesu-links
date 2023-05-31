using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Kafka.Dotnet.Exceptions;
using Rinkudesu.Services.Links.MessageQueues.Messages;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories;
using Rinkudesu.Services.Links.Repositories.Exceptions;
using Rinkudesu.Services.Links.Repositories.QueryModels;
using Xunit;

namespace Rinkudesu.Services.Links.Tests
{
    public class LinkRepositoryTests : ContextTests
    {
        private static readonly Guid _userId = Guid.NewGuid();
        private readonly Mock<IKafkaProducer> _mockKafkaHandler = new();

        public LinkRepositoryTests()
        {
            _mockKafkaHandler.Setup(k => k.Produce(It.IsAny<string>(), It.IsAny<LinkMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        private List<Link> links = new List<Link>();

        private async Task PopulateLinksAsync()
        {
            links = new List<Link>
            {
                new Link { CreatingUserId = _userId, LinkUrl = Guid.NewGuid().ToString() },
                new Link { LinkUrl = "http://localhost/", CreatingUserId = _userId },
                new Link { Title = "ayaya", ShareableKey = "test", CreatingUserId = _userId, LinkUrl = Guid.NewGuid().ToString() },
                new Link { Description = "tuturu*", CreatingUserId = Guid.NewGuid(), LinkUrl = Guid.NewGuid().ToString() },
                new Link { PrivacyOptions = Link.LinkPrivacyOptions.Public, CreatingUserId = Guid.NewGuid(), LinkUrl = Guid.NewGuid().ToString() },
                new Link { CreatingUserId = _userId, LinkUrl = Guid.NewGuid().ToString() }
            };
            _context.Links.AddRange(links);
            await _context.SaveChangesAsync();
        }

        private LinkRepository CreateRepository()
        {
            return new LinkRepository(_context, new NullLogger<LinkRepository>(), _mockKafkaHandler.Object);
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
            var requestGuid = links.First(l => l.CreatingUserId == _userId).Id;

            var result = await repo.GetLinkAsync(requestGuid, _userId);

            Assert.Equal(requestGuid, result.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("f1f854d8-bc3c-4fd3-8583-155afd2a403a")]
        public async Task LinkRepositoryGetLink_RequestedPrivateLinkWithInvalidUserId_DataNotFoundThrown(string? userId)
        {
            Guid? guid = userId is not null ? new Guid(userId) : null;
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var requestGuid = links.First(l => l.CreatingUserId == _userId).Id;

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.GetLinkAsync(requestGuid, guid));
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
        public async Task LinkRepositoryCreateLink_NewLinkWithDuplicateData_DataAlreadyExistsThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            _context.ClearTracked();
            var link = new Link { LinkUrl = links.First().LinkUrl, CreatingUserId = links.First().CreatingUserId };

            await Assert.ThrowsAsync<DataAlreadyExistsException>(() => repo.CreateLinkAsync(link));
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateExistingLinkWithValidUserId_LinkChanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == _userId);
            var updatedLink = new Link
            {
                Id = link.Id,
                Description = "test",
                CreatingUserId = _userId //TODO: remove this assignment as this should not be changeable here
            };
            _context.ClearTracked();

            await repo.UpdateLinkAsync(updatedLink, _userId);

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Equal(updatedLink.Description, dbLink?.Description);
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateExistingLinkWithInvalidUserId_DataNotFoundThrownAndLinkUnchanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == _userId);
            link.Description = "test";
            _context.ClearTracked();

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.UpdateLinkAsync(link, Guid.NewGuid()));

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Null(dbLink?.Description);
        }

        [Fact]
        public async Task LinkRepositoryUpdateLink_UpdateLinkWithInvalidId_DataNotFoundThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = new Link { CreatingUserId = _userId };

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.UpdateLinkAsync(link, _userId));
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteExistingLinkWithValidUserId_LinkDeleted()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == _userId);
            _context.ClearTracked();

            await repo.DeleteLinkAsync(link.Id, _userId);

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.Null(dbLink);
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteExistingLinkWithInvalidUserId_DataNotFoundThrownAndLinkUnchanged()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == _userId);
            link.Description = "test";
            _context.ClearTracked();

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.DeleteLinkAsync(link.Id, Guid.NewGuid()));

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.NotNull(dbLink);
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteLinkWithInvalidId_DataNotFoundThrown()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();
            var link = new Link { CreatingUserId = _userId };

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.DeleteLinkAsync(link.Id, _userId));
        }

        [Fact]
        public async Task CreateLink_CreationAndUpdateDatesSet_CustomValuesIgnored()
        {
            var repo = CreateRepository();
            var link = new Link { CreationDate = DateTime.MinValue, LastUpdate = DateTime.MaxValue };

            await repo.CreateLinkAsync(link);

            var dbLink = _context.Links.First();
            Assert.NotEqual(DateTime.MinValue, dbLink.CreationDate);
            Assert.NotEqual(DateTime.MaxValue, dbLink.LastUpdate);
        }

        [Fact]
        public async Task UpdateLink_CreationAndUpdateDatesSet_CustomValuesIgnored()
        {
            var userId = Guid.NewGuid();
            _context.Links.Add(new Link { CreationDate = DateTime.MinValue, CreatingUserId = userId });
            await _context.SaveChangesAsync();
            var id = _context.Links.First().Id;
            _context.ClearTracked();
            var repo = CreateRepository();
            var link = new Link { Id = id, CreationDate = DateTime.MaxValue, LastUpdate = DateTime.MinValue };

            await repo.UpdateLinkAsync(link, userId);

            var updated = _context.Links.First();
            Assert.Equal(DateTime.MinValue, updated.CreationDate);
            Assert.NotEqual(DateTime.MaxValue, updated.LastUpdate);
        }

        [Theory]
        [InlineData("test")]
        public async Task GetLinkByKeyAsync_NoMatchingLink_Thrown(string keyTest)
        {
            var repo = CreateRepository();

            await Assert.ThrowsAsync<DataNotFoundException>(() => repo.GetLinkByKeyAsync(keyTest));
        }

        [Fact]
        public async Task GetLinkByKeyAsync_LinkExists_Returns()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();

            var result = await repo.GetLinkByKeyAsync("test");

            Assert.Equal("test", result.ShareableKey);
        }

        [Fact]
        public async Task LinkRepositoryCreateLink_NewLinkQueuePublishFails_LinkNotAdded()
        {
            _mockKafkaHandler.Setup(k => k.Produce(It.IsAny<string>(), It.IsAny<LinkMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KafkaProduceException());
            var repo = CreateRepository();
            var link = new Link();

            await Assert.ThrowsAsync<KafkaProduceException>(() => repo.CreateLinkAsync(link));

            Assert.Empty(_context.Links);
        }

        [Fact]
        public async Task LinkRepositoryDeleteLink_DeleteExistingLinkWithValidUserIdFailedToPublishToKafka_LinkNotDeleted()
        {
            await PopulateLinksAsync();
            _mockKafkaHandler.Setup(k => k.Produce(It.IsAny<string>(), It.IsAny<LinkMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KafkaProduceException());
            var repo = CreateRepository();
            var link = links.First(l => l.CreatingUserId == _userId);
            _context.ClearTracked();

            await Assert.ThrowsAsync<KafkaProduceException>(() => repo.DeleteLinkAsync(link.Id, _userId));

            var dbLink = await _context.Links.FindAsync(link.Id);
            Assert.NotNull(dbLink);
        }

        [Fact]
        public async Task ForceDeleteAllUserLinks_NoLinksForUser_RunsAndDeletesNothing()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();

            await repo.ForceRemoveAllUserLinks(Guid.NewGuid());

            Assert.Equal(links.Count, await _context.Links.CountAsync());
        }

        [Fact]
        public async Task ForceDeleteAllUserLinks_LinksExistForUser_DeletesAllUserLinks()
        {
            await PopulateLinksAsync();
            var repo = CreateRepository();

            await repo.ForceRemoveAllUserLinks(_userId);

            Assert.DoesNotContain(_context.Links, l => l.CreatingUserId == _userId);
        }
    }
}
