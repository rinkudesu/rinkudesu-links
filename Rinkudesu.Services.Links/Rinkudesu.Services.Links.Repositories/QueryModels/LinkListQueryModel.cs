using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.Repositories.QueryModels
{
    public class LinkListQueryModel
    {
        /// <summary>
        /// UserId creating the link
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// String that has to be contained in Url
        /// </summary>
        [SuppressMessage("Design", "CA1056", MessageId = "URI-like properties should not be strings")]
        public string? UrlContains { get; set; }
        /// <summary>
        /// String that has to be contained in the title
        /// </summary>
        public string? TitleContains { get; set; }
        /// <summary>
        /// Shows private links
        /// </summary>
        public bool ShowPrivate { get; set; }
        /// <summary>
        /// Shows public links
        /// </summary>
        public bool ShowPublic { get; set; } = true;
        /// <summary>
        /// Sorts the list in a descending order
        /// </summary>
        public bool SortDescending { get; set; }
        /// <summary>
        /// Selects the sort order for the list
        /// </summary>
        public LinkListSortOptions SortOptions { get; set; }

        public IQueryable<Link> ApplyQueryModel(IQueryable<Link> links)
        {
            SanitizeModel();
            links = FilterUserId(links);
            links = FilterUrlContains(links);
            links = FilterTitleContains(links);
            links = FilterVisibility(links);
            links = SortLinks(links);
            return links;
        }

        public void SanitizeModel()
        {
            UserId = UserId?.Trim();
            UrlContains = UrlContains?.Trim();
            TitleContains = TitleContains?.Trim();
            if (UserId is null)
            {
                ShowPrivate = false;
            }
        }

        public IQueryable<Link> FilterUserId(IQueryable<Link> links)
        {
            if (UserId != null)
            {
                return links.Where(l => l.CreatingUserId == UserId || l.PrivacyOptions == Link.LinkPrivacyOptions.Public);
            }
            return links;
        }

        public IQueryable<Link> FilterUrlContains(IQueryable<Link> links)
        {
            if (UrlContains != null)
            {
                return links.Where(l => l.LinkUrl.Contains(UrlContains));
            }
            return links;
        }

        public IQueryable<Link> FilterTitleContains(IQueryable<Link> links)
        {
            if (TitleContains != null)
            {
                return links.Where(l => l.Title.Contains(TitleContains));
            }
            return links;
        }

        public IQueryable<Link> FilterVisibility(IQueryable<Link> links)
        {
            if (!ShowPrivate)
            {
                links = links.Where(l => l.PrivacyOptions != Link.LinkPrivacyOptions.Private);
            }
            if (!ShowPublic)
            {
                links = links.Where(l => l.PrivacyOptions != Link.LinkPrivacyOptions.Public);
            }
            return links;
        }

        public IQueryable<Link> SortLinks(IQueryable<Link> links) =>
            SortOptions switch
            {
                LinkListSortOptions.Title => SortDescending
                    ? links.OrderByDescending(l => l.Title)
                    : links.OrderBy(l => l.Title),
                LinkListSortOptions.Url => SortDescending
                    ? links.OrderByDescending(l => l.LinkUrl)
                    : links.OrderBy(l => l.LinkUrl),
                LinkListSortOptions.CreationDate => SortDescending
                    ? links.OrderByDescending(l => l.CreationDate)
                    : links.OrderBy(l => l.CreationDate),
                LinkListSortOptions.UpdateDate => SortDescending
                    ? links.OrderByDescending(l => l.LastUpdate)
                    : links.OrderBy(l => l.LastUpdate),
                _ => links.OrderBy(l => l.Id)
            };

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

    public enum LinkListSortOptions
    {
        Title,
        Url,
        CreationDate,
        UpdateDate
    }
}