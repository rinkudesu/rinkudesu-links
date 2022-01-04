using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Services.Links.Utilities;

namespace Rinkudesu.Services.Links.Models
{
    [ExcludeFromCodeCoverage]
    public class Link
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DataType(DataType.Url)]
        [Required]
        [SuppressMessage("Design", "CA1056", MessageId = "URI-like properties should not be strings")]
        [MaxLength(200)]
        public string LinkUrl { get; set; }
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
        [MinLength(51)]
        [MaxLength(51)]
        public string? ShareableKey { get; set; }

        public Link()
        {
            CreationDate = DateTime.UtcNow;
            LastUpdate = CreationDate;
            LinkUrl = string.Empty;
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