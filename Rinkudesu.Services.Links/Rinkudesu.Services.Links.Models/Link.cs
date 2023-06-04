using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Models
{
    [ExcludeFromCodeCoverage]
    [Index(nameof(LinkUrl), nameof(CreatingUserId), IsUnique = true)]
    public class Link
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DataType(DataType.Url)]
        [Required]
        [MaxLength(200)]
        public Uri LinkUrl { get; set; } = null!;
        /// <summary>
        /// This field can be safely used to filter and sort links.
        /// </summary>
        /// <remarks>
        /// Please note that this field is set by the database, so it will not have a value before it's saved and then retrieved.
        /// </remarks>
        [MaxLength(200)]
        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings")]
        public string SearchableUrl { get; private set; } = null!;
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(250)]
        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        [MaxLength(1000)]
        public string? Description { get; set; }
        public LinkPrivacyOptions PrivacyOptions { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastUpdate { get; set; }
        public Guid CreatingUserId { get; set; }
        [MaxLength(51)]
        public string? ShareableKey { get; set; }

        public Link()
        {
            CreationDate = DateTime.UtcNow;
            LastUpdate = CreationDate;
            Title = string.Empty;
            CreatingUserId = Guid.Empty;
        }

        public void SetCreateDates(DateTime? creationTimeUtc = null)
        {
            creationTimeUtc ??= DateTime.UtcNow;
            CreationDate = creationTimeUtc.Value;
            SetUpdateDate(creationTimeUtc.Value.SetKindUtc());
        }

        public void SetUpdateDate(DateTime? updateTimeUtc = null)
        {
            updateTimeUtc ??= DateTime.UtcNow;
            LastUpdate = updateTimeUtc.Value.SetKindUtc();
        }

        public void Update(Link newLink)
        {
            LinkUrl = newLink.LinkUrl;
            Title = newLink.Title;
            Description = newLink.Description;
            PrivacyOptions = newLink.PrivacyOptions;
            SetUpdateDate();
        }

        /// <summary>
        /// Generates a new value for <see cref="ShareableKey"/>, unless it already exists and <paramref name="regenerate"/> is <see langword="false"/>
        /// </summary>
        /// <returns>Generated key value</returns>
        public string GenerateShareableKey(bool regenerate = true)
        {
            if (ShareableKey is not null && !regenerate)
            {
                return ShareableKey;
            }

            // this 36 bytes will return a base64 string with length of 48
            // this will also remove / and + from base64 as they are kinda important url characters
            ShareableKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(36)).Replace("/", "_", StringComparison.Ordinal).Replace("+", "-", StringComparison.Ordinal);
            return ShareableKey;
        }

        public void Unshare() => ShareableKey = null;

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }

        public enum LinkPrivacyOptions
        {
            Private,
            Public
        }
    }
}
