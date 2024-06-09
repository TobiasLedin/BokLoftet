using BokLoftet.Data;
using BokLoftet.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing.Text;
using Xunit;

namespace BokLoftet.Test
{
    public class DbTests : IAsyncLifetime
    {
        private readonly TestServer _server;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbTests()
        {
            var builder = new WebHostBuilder().UseStartup<DbTestStartup>();

            _server = new TestServer(builder);
            _serviceProvider = _server.Host.Services;

            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        }


        [Fact]
        public void DB_CheckIfCategoryExists()
        {
            var category = _context.Categories.FirstOrDefault(x => x.Name == "Barnb�cker");

            Assert.NotNull(category);

        }

        

        public async Task InitializeAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await SeedData();
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
        }

        public async Task SeedData()
        {
            // Categories
            var categories = new List<Category>
            {
                new Category { Name = "Barnb�cker" },
                new Category { Name = "Thriller" }
            };
            _context.Categories.AddRange(categories);

            // Books
            var books = new List<Book>
            {
                new Book
                {
                    Author = "Astrid Lindgren",
                    Category = categories[0],
                    Title = "Pippi L�ngstrump",
                    Description = "En festlig bok om en stark liten flicka.",
                    Language = "Svenska",
                    Publisher = "Bonnier",
                    PublishYear = 1948,
                    Pages = 60,
                    ISBN = "9789129697285",
                    CoverImageURL = ""
                },
                new Book
                {
                    Author = "Astrid Lindgren",
                    Category = categories[0],
                    Title = "Pippi L�ngstrump",
                    Description = "En festlig bok om en stark liten flicka.",
                    Language = "Svenska",
                    Publisher = "Bonnier",
                    PublishYear = 1948,
                    Pages = 60,
                    ISBN = "9789129697285",
                    CoverImageURL = ""
                }
            };
            _context.Books.AddRange(books);

            // Roles
            var roles = new List<string> { "Customer", "Admin" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // User
            ApplicationUser user1 = new()
            {
                FirstName = "Janne",
                LastName = "Karlsson",
                Adress = "Blomv�gen 1, G�teborg",
                Email = "janneloffe@karlsson.se",
                NormalizedEmail = "JANNELOFFE@KARLSSON.SE",
                PhoneNumber = "555 123 456",
                UserName = "janneloffe@karlsson.se"
            };

            ApplicationUser user2 = new()
            {
                FirstName = "Greta",
                LastName = "Svensson",
                Adress = "Ringv�gen 1, G�teborg",
                Email = "greta@bokloftet.se",
                NormalizedEmail = "GRETA@BOKLOFTET.SE",
                PhoneNumber = "555 123 457",
                UserName = "greta@bokloftet.se"
            };

            await _userManager.CreateAsync(user1, "Test123!");
            await _userManager.AddToRoleAsync(user1, "Customer");

            await _userManager.CreateAsync(user2, "Test123!");
            await _userManager.AddToRoleAsync(user2, "Admin");

            _context.SaveChanges();
        }

        [Fact]
        public void LoanBook()
        {
            //Arrange
            var book = _context.Books.FirstOrDefault(b => b.IsAvailable);
            var user = _context.Users.FirstOrDefault();
            Order order = new Order();

            Assert.NotNull(book);
            Assert.NotNull(user);

            //Act
            //Run the Loan book method

            //Assert
            Assert.False(book.IsAvailable, "Boken b�r markeras som otillg�nglig");
            Assert.Contains(book, order.Books);
        }

        [Fact]
        public void ReturnBook()
        {
            //Arrange
            var order = _context.Orders.FirstOrDefault();
            var user = order.Customer;
            var book = order.Books.FirstOrDefault();

            //Act
            //Run the Return book method

            //Assert
            Assert.True(book.IsAvailable, "Boken b�r markeras som otillg�nglig");
        }
    }
}