using System;
using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.DataTransferObjects
{
    [ExcludeFromCodeCoverage]
    public class LinkDto
    {
        /// <summary>
        /// Link id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// URL the link is pointing to
        /// </summary>
        public string LinkUrl { get; set; } = string.Empty;
        /// <summary>
        /// Title of the link
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Description of the link
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Privacy configuration of the link
        /// </summary>
        public Link.LinkPrivacyOptions PrivacyOptions { get; set; }
        /// <summary>
        /// Date the link was created at
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Date the link was last modified at
        /// </summary>
        public DateTime LastUpdate { get; set; }
        /// <summary>
        /// Id of a user creating the link
        /// </summary>
        public string CreatingUserId { get; set; } = string.Empty;
    }
}