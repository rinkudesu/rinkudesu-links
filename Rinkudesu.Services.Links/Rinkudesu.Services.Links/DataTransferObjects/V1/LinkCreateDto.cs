using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Services.Links.Models;

namespace Rinkudesu.Services.Links.DataTransferObjects.V1
{
    /// <summary>
    /// Data transfer object to send and receive <see cref="Link"/> objects
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LinkCreateDto
    {
        /// <summary>
        /// URL the link is pointing to
        /// </summary>
        [DataType(DataType.Url)]
        [MaxLength(200)]
        public string LinkUrl { get; set; } = null!;
        /// <summary>
        /// Title of the link
        /// </summary>
        [MaxLength(250)]
        public string Title { get; set; } = null!;
        /// <summary>
        /// Description of the link
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
        /// <summary>
        /// Privacy configuration of the link
        /// </summary>
        public Link.LinkPrivacyOptions PrivacyOptions { get; set; }

        /// <summary>
        /// Verifies whether <see cref="LinkUrl"/> is a valid absolute url.
        /// </summary>
        public bool IsLinkUrlValid() => Uri.TryCreate(LinkUrl, UriKind.Absolute, out _);
    }
}
