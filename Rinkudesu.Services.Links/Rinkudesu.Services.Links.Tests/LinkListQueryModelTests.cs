using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rinkudesu.Gateways.Utils;
using Rinkudesu.Services.Links.Models;
using Rinkudesu.Services.Links.Repositories.QueryModels;
using Xunit;

namespace Rinkudesu.Services.Links.Tests
{
    public class LinkListQueryModelTests
    {
        private static readonly Guid _userId = Guid.NewGuid();

        private List<Link> links = new List<Link>();
        private void PopulateLinks()
        {
            links = new List<Link>
            {
                new Link { LinkUrl = Guid.NewGuid().ToString().ToUri() },
                new Link { LinkUrl = "http://localhost/".ToUri() },
                new Link { Title = "ayaya", LinkUrl = Guid.NewGuid().ToString().ToUri() },
                new Link { Description = "tuturu*", LinkUrl = Guid.NewGuid().ToString().ToUri() },
                new Link { PrivacyOptions = Link.LinkPrivacyOptions.Public, LinkUrl = Guid.NewGuid().ToString().ToUri() },
                new Link { CreatingUserId = _userId, LinkUrl = Guid.NewGuid().ToString().ToUri() },
            };
            foreach (var link in links)
            {
                FillSearchableUrl(link);
            }
        }

        [Fact]
        public void LinkListQueryModelSanitizeModel_WhiteSpacesInFields_WhitespacesTrimmed()
        {
            var model = new LinkListQueryModel
            {
                UserId = _userId,
                UrlContains = "          test                    ",
                TitleContains = "               test"
            };

            model.SanitizeModel();

            Assert.Equal(_userId, model.UserId);
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
            var model = new LinkListQueryModel { UserId = Guid.NewGuid() };

            var result = model.FilterUserId(links.AsQueryable());

            Assert.Single(result);
            Assert.Equal(links.First(l => l.PrivacyOptions == Link.LinkPrivacyOptions.Public).Id, result.First().Id);
        }

        [Fact]
        public void LinkListQueryModelFilteredUserId_UserIdMatchingOnce_ReturnsPublicAndValidLinks()
        {
            PopulateLinks();
            var model = new LinkListQueryModel { UserId = _userId };

            var result = model.FilterUserId(links.AsQueryable());

            Assert.Equal(2, result.Count());
            Assert.Contains(result, l => l.CreatingUserId == _userId);
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
            Assert.Contains(url, result.First().LinkUrl.ToString(), StringComparison.InvariantCultureIgnoreCase);
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
                    Title = "a", LinkUrl = "z".ToUri(), CreationDate = DateTime.Now.AddDays(-6),
                    LastUpdate = DateTime.Now.AddDays(-10)
                },
                new Link
                {
                    Title = "e", LinkUrl = "a".ToUri(), CreationDate = DateTime.Now.AddDays(-3),
                    LastUpdate = DateTime.Now.AddDays(-2)
                },
                new Link
                {
                    Title = "j", LinkUrl = "s".ToUri(), CreationDate = DateTime.Now.AddDays(-2),
                    LastUpdate = DateTime.Now.AddDays(-4)
                },
                new Link
                {
                    Title = "b", LinkUrl = "i".ToUri(), CreationDate = DateTime.Now.AddDays(-4),
                    LastUpdate = DateTime.Now.AddDays(-1)
                },
                new Link
                {
                    Title = "z", LinkUrl = "j".ToUri(), CreationDate = DateTime.Now.AddDays(-9), LastUpdate = DateTime.Now
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortAscByUrl_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Url };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.SearchableUrl).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
            }
        }

        [Fact]
        public void LinkListQueryModelSortLinks_SortDescByUrl_SortedCorrectly()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { SortOptions = LinkListSortOptions.Url, SortDescending = true };

            var result = model.SortLinks(testLinks.AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderByDescending(l => l.SearchableUrl).ToList();
            for (int i = 0; i < testLinks.Count; i++)
            {
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
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
                Assert.Equal(sortedLinks[i].Title, result[i].Title);
            }
        }

        [Fact]
        public void LinkListQueryModelSkipTake_ValuesProvided_ReturnedCorrectCollection()
        {
            var testLinks = GetSortTestLinks();
            var model = new LinkListQueryModel { Skip = 1, Take = 2 };

            var result = model.SkipTake(testLinks.OrderBy(l => l.Title).AsQueryable()).ToList();

            var sortedLinks = testLinks.OrderBy(l => l.Title).ToList();
            Assert.DoesNotContain(result, l => l.Title == sortedLinks[0].Title);
            Assert.Equal(2, result.Count);
            for (int i = 1; i < 3; i++)
            {
                Assert.Equal(sortedLinks[i].Title, result[i - 1].Title);
            }
        }

        // this uses reflections as normally this would be set by the database and doing anything to allow this field to be set programmatically would be "tests only" anyway
        private static void FillSearchableUrl(Link link)
        {
            var property = link.GetType().GetProperty(nameof(Link.SearchableUrl));
            property!.SetValue(link, link.LinkUrl.ToString().Replace("https://", string.Empty).Replace("http://", string.Empty).ToUpperInvariant());
        }
    }
}
