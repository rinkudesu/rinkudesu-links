using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

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
        public string LinkUrl { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(250)]
        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        public LinkPrivacyOptions PrivacyOptions { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastUpdate { get; set; }
        public string CreatingUserId { get; set; }

        public Link()
        {
            CreationDate = DateTime.Now;
            LastUpdate = CreationDate;
            LinkUrl = string.Empty;
            Title = string.Empty;
            CreatingUserId = string.Empty;
        }

        public void SetCreateDates(DateTime? creationTime = null)
        {
            creationTime ??= DateTime.Now;
            CreationDate = creationTime.Value;
            SetUpdateDate(creationTime.Value);
        }

        public void SetUpdateDate(DateTime? updateTime = null)
        {
            updateTime ??= DateTime.Now;
            LastUpdate = updateTime.Value;
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