using System.ComponentModel.DataAnnotations;

namespace BokLoftet.Models
{
    public class Order
    {
        public int Id { get; set; }
        public List<Book> Books { get; set; }
        [Required]
        public ApplicationUser Customer { get; set; }
        public DateTime LoanDate { get; set; } = DateTime.Now.Date;
        public DateTime ReturnDate { get; set; } = DateTime.Now.Date.AddDays(30);

    }
}
