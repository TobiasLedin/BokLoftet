using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BokLoftet.Models
{
    public class Book
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; } = true;
        [Required]
        public string Title { get; set; }
        [Required]
        public Category Category { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string Language { get; set; }
        [Required]
        public int Pages { get; set; }
        [Required]
        public string Publisher { get; set; }
        [Required]
        public int PublishYear { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Description { get; set; }
        public string CoverImageURL { get; set; }

    }
}
