using BokLoftet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BokLoftet.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Book> Books { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(

                 new IdentityRole
                 {
                     Name = "Admin",
                     NormalizedName = "ADMIN",
                     Id = "0713623c-b540-46ca-9004-3499c0004d02"
                 },

                new IdentityRole
                {
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    Id = "530a8ef5-c869-43dd-9129-c1b16291b7b8"
                });

        }
    }
}
