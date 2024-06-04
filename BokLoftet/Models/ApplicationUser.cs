using Microsoft.AspNetCore.Identity;

namespace BokLoftet.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Adress { get; set; }
    }
}
