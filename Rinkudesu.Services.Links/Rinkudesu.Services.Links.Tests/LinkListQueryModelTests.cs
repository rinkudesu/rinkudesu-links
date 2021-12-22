using System;
using System.Collections.Generic;
using System.Linq;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.QueryModels;
using Xunit;

namespace Rinkudesu.Services.Links.Tests
{
    public class LinkListQueryModelTests
    {
        private List<Link> links = new List<Link>();
        private void PopulateLinks()
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
        }

        [Fact]
        public void LinkListQueryModelSanitizeModel_WhiteSpacesInFields_WhitespacesTrimmed()
        {
            var model = new LinkListQueryModel
            {
                UserId = "   test       ",
                UrlContains = "          test                    ",
                TitleContains = "               test"
            };

            model.SanitizeModel();

            Assert.Equal("test", model.UserId);
            Assert.Equal("test", model.UrlContains);
            Assert.Equal("test", model.TitleContains);
        }

        [Fact]
        public void LinkListQueryModelSanitizeModel_ShowPrivateWithNoUserId_ShowPrivateSetToFalse()
        {
            var model = new LinkListQueryModel { ShowPublic = true };

            model.SanitizeModel();

            Assert.False(model.ShowPrivate);
        }

        [Fact]
        public void LinkListQueryModelFilterUserId_UserIdNull_ReturnsOriginalCollection()
        {
            PopulateLinks();
            var model = new LinkListQueryModel();

            var result = model.FilterUserId(links.AsQueryable());

            Assert.Equal(links.Count, result.Count());
            foreach (var link in links)
            {
                Assert.Contains(result, l => l.Id == link.Id);
            }
        }

        [Fact]
        public void LinkListQueryModelFilterUserId_UserIdNotInAnyLink_ReturnsPublicOnly()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { UserId = "invalid" };

            var result = model.FilterUserId(links.AsQueryable());

            Assert.Single(result);
            Assert.Equal(links.First(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public).Id, result.First().Id);
        }

        [Fact]
        public void LinkListQueryModelFilteredUserId_UserIdMatchingOnce_ReturnsPublicAndValidLinks()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { UserId = "a" };

            var result = model.FilterUserId(links.AsQueryable());

            Assert.Equal(2, result.Count());
            Assert.Contains(result, l => l.CreatingUserId == "a");
            Assert.Contains(result, l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public);
        }

        [Fact]
        public void LinkListQueryModelFilterUrlContains_UrlContainsNull_ReturnsOriginalCollection()
        {
            PopulateLinks();
            var model = new LinkListQueryModel();

            var result = model.FilterUrlContains(links.AsQueryable());

            Assert.Equal(links.Count, result.Count());
            foreach (var link in links)
            {
                Assert.Contains(result, l => l.Id == link.Id);
            }
        }

        [Fact]
        public void LinkListQueryModelFilterUrlContains_UrlContainsNotInAnyLink_ReturnsEmptyQueryable()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { UrlContains = "invalid" };

            var result = model.FilterUrlContains(links.AsQueryable());

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("local")]
        [InlineData("localhost")]
        [InlineData("http://localhost/")]
        public void LinkListQueryModelFilterUrlContains_UrlContainsExistingValue_SingleLinkReturned(string url)
        {
            PopulateLinks();
            var model = new LinkListQueryModel { UrlContains = url };

            var result = model.FilterUrlContains(links.AsQueryable());

            Assert.Single(result);
            Assert.Contains(url, result.First().LinkUrl, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void LinkListQueryModelFilterTitleContains_TitleContainsNull_ReturnsOriginalCollection()
        {
            PopulateLinks();
            var model = new LinkListQueryModel();

            var result = model.FilterTitleContains(links.AsQueryable());

            Assert.Equal(links.Count, result.Count());
            foreach (var link in links)
            {
                Assert.Contains(result, l => l.Id == link.Id);
            }
        }

        [Fact]
        public void LinkListQueryModelFilterTitleContains_TitleContainsNotInAnyLink_ReturnsEmptyQueryable()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { TitleContains = "invalid" };

            var result = model.FilterTitleContains(links.AsQueryable());

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("aya")]
        [InlineData("a")]
        [InlineData("ayaya")]
        public void LinkListQueryModelFilterTitleContains_TitleContainsExistingValue_SingleLinkReturned(string ayaya)
        {
            PopulateLinks();
            var model = new LinkListQueryModel { TitleContains = ayaya };

            var result = model.FilterTitleContains(links.AsQueryable());

            Assert.Single(result);
            Assert.Contains(ayaya, result.First().Title, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void LinkListQueryModelFilterFilterVisibility_BothFalse_EmptyQueryable()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { ShowPublic = false };

            var result = model.FilterVisibility(links.AsQueryable());

            Assert.Empty(result);
        }

        [Fact]
        public void LinkListQueryModelFilterVisibility_ShowPrivate_OnlyPrivateReturned()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { ShowPrivate = true, ShowPublic = false };

            var result = model.FilterVisibility(links.AsQueryable());

            Assert.Equal(links.Count(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Private), result.Count());
            Assert.DoesNotContain(result, l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public);
        }

        [Fact]
        public void LinkListQueryModelFilterVisibility_ShowPublic_OnlyPublicReturned()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { ShowPublic = true };

            var result = model.FilterVisibility(links.AsQueryable());

            Assert.Equal(links.Count(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public), result.Count());
            Assert.DoesNotContain(result, l => l.PrivacyOptions == Link.LinkPrivacyOptions.Private);
        }

        private static List<Link> GetSortTestLinks()
        {
            return new List<Link>
            {
                new Link
                {
                    Title = "a", LinkUrl = "z", CreationDate = DateTime.Now.AddDays(-6),
                    LastUpdate = DateTime.Now.AddDays(-10)
                },
                new Link
                {
                    Title = "e", LinkUrl = "a", CreationDate = DateTime.Now.AddDays(-3),
                    LastUpdate = DateTime.Now.AddDays(-2)
                },
                new Link
                {
                    Title = "j", LinkUrl = "s", CreationDate = DateTime.Now.AddDays(-2),
                    LastUpdate = DateTime.Now.AddDays(-4)
                },
                new Link
                {
                    Title = "b", LinkUrl = "i", CreationDate = DateTime.Now.AddDays(-4),
                    LastUpdate = DateTime.Now.AddDays(-1)
                },
                new Link
                {
                    Title = "z", LinkUrl = "j", CreationDate = DateTime.Now.AddDays(-9), LastUpdate = DateTime.Now
                },
            };
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortAscByTitle_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Title };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.Title).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortDescByTitle_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Title, SortDescending = true };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderByDescending(l => l.Title).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortAscByUrl_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Url };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.LinkUrl).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortDescByUrl_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Url, SortDescending = true };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderByDescending(l => l.LinkUrl).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortAscByCreationDate_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.CreationDate };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.CreationDate).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortDescByCreationDate_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.CreationDate, SortDescending = true };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderByDescending(l => l.CreationDate).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortAscByUpdateDate_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.UpdateDate };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.LastUpdate).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortDescByUpdateDate_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.UpdateDate, SortDescending = true };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderByDescending(l => l.LastUpdate).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortByDefault_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = (LinkListSortOptions)(-1) };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.Id).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Id, result[i].Id);
            }
        }
    }
}