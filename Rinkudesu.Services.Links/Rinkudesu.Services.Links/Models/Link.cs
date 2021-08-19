using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rinkudesu.Services.Links.Models
{
    public class Link
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DataType(DataType.Url)]
        [Required]
        public string? LinkUrl { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(250)]
        public string? Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        public LinkPrivacyOptions PrivacyOptions { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime LastUpdate { get; set; }
        [Required]
        public string? CreatingUserId { get; set; }

        public Link()
        {
            CreationDate = DateTime.Now;
            LastUpdate = CreationDate;
        }

        public enum LinkPrivacyOptions
        {
            Private,
            Public
        }
    }
}