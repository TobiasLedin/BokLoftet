using BokLoftet.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BokLoftet.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        [DisplayName("Titel")]
        public string Title { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [DisplayName("Författare")]
        public string Author { get; set; }

        [Required]
        [DisplayName("Språk")]
        public string Language { get; set; }

        [Required]
        [DisplayName("Sidor")]
        public int Pages { get; set; }

        [Required]
        [DisplayName("Förläggare")]
        public string Publisher { get; set; }

        [Required]
        [DisplayName("År")]
        public int PublishYear { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        [DisplayName("Beskrivning")]
        public string Description { get; set; }

        [DisplayName("Omslagsbild")]
        public string CoverImageURL { get; set; }
    }
}
