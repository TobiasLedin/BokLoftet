using BokLoftet.Data;
using BokLoftet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BokLoftet.Test
{
    public class BokLoftetTests : IAsyncLifetime
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BokLoftetTests(UserManager<ApplicationUser> userManager)
        {
            _context = CreateDbContext();
            _userManager = userManager;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void DB_CheckIfCategoryDeckareExists()
        {
            var category = _context.Categories.FirstOrDefault(x => x.Name == "Barnböcker");

            Assert.NotNull(category);

            // Arrage
            //_context.Categories.Add(new Category { Name = "Test" });
            //_context.SaveChanges();



            //// Act
            //var test = _context.Database.CanConnect();
            //var test1 = _context.Categories.FirstOrDefault(x => x.Name == "Test");

            //Assert.True(test);
            //Assert.NotNull(test1);
        }

        private async Task SeedData()
        {
            // Categories
            List<Category> categories = [];

            //categories.Add(new Category { Name = "Deckare" });
            //categories.Add(new Category { Name = "Fantasy" });
            //categories.Add(new Category { Name = "Barnböcker" });
            //categories.Add(new Category { Name = "Skönlitteratur" });
            //categories.Add(new Category { Name = "Thriller" });

            var childrensBooks = new Category { Name = "Barnböcker" };
            var thriller = new Category { Name = "Thriller" };

            categories.Add(thriller);
            categories.Add(childrensBooks);

            _context.Categories.AddRange(categories);
            
            // Books
            List<Book> books = [];

            books.Add(new Book
            {
                Author = "Astrid Lindgren",
                /*Category = _context.Categories.First(x => x.Name == "Barnböcker")*/ // Fungerar ej
                Category = childrensBooks,
                Title = "Pippi Långstrump",
                Description = "En festlig bok om en stark liten flicka.",
                Language = "Svenska",
                Publisher = "Bonnier",
                PublishYear = 1948,
                Pages = 60,
                ISBN = "9789129697285",
                CoverImageURL = ""
            });

            books.Add(new Book
            {
                Author = "Lee Child",
                /*Category = _context.Categories.First(x => x.Name == "Thriller")*/ // Fungerar ej
                Category = thriller,
                Title = "Jack Reacher",
                Description = "En spännande bok en föredetta militärpolis.",
                Language = "Engelska",
                Publisher = "Bantam Press",
                PublishYear = 1997,
                Pages = 592,
                ISBN = "0-515-12344-7",
                CoverImageURL = ""
            });

            _context.Books.AddRange(books);

            //User

            ApplicationUser user1 = new()
            {
                FirstName = "Janne",
                LastName = "Karlsson",
                Adress = "Blomvägen 1, Göteborg",
                Email = "janneloffe@karlsson.se",
                NormalizedEmail = "JANNELOFFE@KARLSSON.SE",
                PhoneNumber = "555 123 456",
                UserName = "janneloffe@karlsson.se"

            };

            await _userManager.CreateAsync(user1, "Test123!");
            await _userManager.AddToRoleAsync(user1, "Customer");

            ApplicationUser user2 = new()
            {
                FirstName = "Greta",
                LastName = "Svensson",
                Adress = "Ringvägen 1, Göteborg",
                Email = "greta@bokloftet.se",
                NormalizedEmail = "GRETA@BOKLOFTET.SE",
                PhoneNumber = "555 123 457",
                UserName = "greta@bokloftet.se"

            };


            await _userManager.CreateAsync(user2, "Test123!");
            await _userManager.AddToRoleAsync(user2, "Admin");

            _context.SaveChanges();
        }

        //private async Task Initial()
        //{
        //    await _context.Database.EnsureCreatedAsync();
        //    SeedData();
        //}

        //public void Dispose()
        //{
        //    _context.Database.EnsureDeleted();
        //}

        public async Task InitializeAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await SeedData();
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
        }
    }
}