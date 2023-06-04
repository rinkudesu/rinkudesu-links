using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.Repositories.QueryModels
{
    public class LinkListQueryModel
    {
        /// <summary>
        /// UserId creating the link
        /// </summary>
        public Guid? UserId { get; set; }
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
        /// <summary>
        /// Limits returned links by applied tags.
        /// </summary>
        /// <remarks>
        /// This filter should be used independently first and then the resulting ids should be passed to <see cref="ApplyQueryModel"/>.
        /// If the user selects multiple tags, the result should be a sum of all links with any of those tags.
        /// </remarks>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
        public Guid[]? TagIds { get; set; }

        [Range(0, int.MaxValue)]
        public int? Skip { get; set; }
        [Range(0, int.MaxValue)]
        public int? Take { get; set; }

        public IQueryable<Link> ApplyQueryModel(IQueryable<Link> links, IEnumerable<Guid>? idsLimit = null)
        {
            SanitizeModel();
            links = FilterUserId(links);
            links = FilterUrlContains(links);
            links = FilterTitleContains(links);
            links = FilterVisibility(links);
            links = SortLinks(links);
            links = SkipTake(links);

            if (idsLimit is not null)
            {
                links = links.Where(l => idsLimit.Contains(l.Id));
            }

            return links;
        }

        public void SanitizeModel()
        {
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
                return links.Where(l => l.SearchableUrl.Contains(UrlContains.ToUpperInvariant()));
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

        public IQueryable<Link> SkipTake(IQueryable<Link> links)
        {
            if (Skip.HasValue)
            {
                links = links.Skip(Skip.Value);
            }
            if (Take.HasValue)
            {
                links = links.Take(Take.Value);
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
                    ? links.OrderByDescending(l => l.SearchableUrl)
                    : links.OrderBy(l => l.SearchableUrl),
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
