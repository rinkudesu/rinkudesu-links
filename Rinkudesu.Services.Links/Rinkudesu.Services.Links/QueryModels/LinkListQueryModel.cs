using System.Linq;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.QueryModels
{
    public class LinkListQueryModel
    {
        public string? UserId { get; set; }
        public string? UrlContains { get; set; }
        public string? TitleContains { get; set; }
        public bool ShowPrivate { get; set; }
        public bool ShowPublic { get; set; }
        public bool SortDescending { get; set; }
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

        private IQueryable<Link> FilterUserId(IQueryable<Link> links)
        {
            if (UserId != null)
            {
                return links.Where(l => l.CreatingUserId == UserId || l.PrivacyOptions == Link.LinkPrivacyOptions.Public);
            }
            return links;
        }

        private IQueryable<Link> FilterUrlContains(IQueryable<Link> links)
        {
            if (UrlContains != null)
            {
                return links.Where(l => l.LinkUrl.Contains(UrlContains));
            }
            return links;
        }

        private IQueryable<Link> FilterTitleContains(IQueryable<Link> links)
        {
            if (TitleContains != null)
            {
                return links.Where(l => l.Title.Contains(TitleContains));
            }
            return links;
        }

        private IQueryable<Link> FilterVisibility(IQueryable<Link> links)
        {
            if (!ShowPrivate)
            {
                return links.Where(l => l.PrivacyOptions != Link.LinkPrivacyOptions.Private);
            }
            if (!ShowPublic)
            {
                return links.Where(l => l.PrivacyOptions != Link.LinkPrivacyOptions.Public);
            }
            return links;
        }

        private IQueryable<Link> SortLinks(IQueryable<Link> links) =>
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