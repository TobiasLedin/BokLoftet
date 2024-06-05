using BokLoftet.Data;
using BokLoftet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BokLoftet.Test
{
    public class BokLoftetTests : IAsyncLifetime
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public BokLoftetTests()
        {

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;


            _context = new ApplicationDbContext(_options);

                _context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

            //var context = new ApplicationDbContext(_options);

            var store = new UserStore<ApplicationUser>(_context);
            var hasher = new PasswordHasher<ApplicationUser>();
            var validators = new[] { new UserValidator<ApplicationUser>() };

            _userManager = new UserManager<ApplicationUser>(store, null, hasher, validators, null, null, null, null, null);

            var roleStore = new RoleStore<IdentityRole>(_context);
            var _roleManager = new RoleManager<IdentityRole>(roleStore, null, null, null, null);

            SeedData(_userManager, _roleManager).Wait();
        }

       

        [Fact]
        public void DB_CheckIfCategoryDeckareExists()
        {
            var category = _context.Categories.FirstOrDefault(x => x.Name == "Barnböcker");

            Assert.NotNull(category);

            // Arrange
            //_context.Categories.Add(new Category { Name = "Test" });
            //_context.SaveChanges();



            //// Act
            //var test = _context.Database.CanConnect();
            //var test1 = _context.Categories.FirstOrDefault(x => x.Name == "Test");

            //Assert.True(test);
            //Assert.NotNull(test1);
        }

        [Fact]
        public void DB_TestFindUserByEmail()
        {
            //arrange
            
            //act
            var user = _userManager.Users.FirstOrDefault(u => u.Email == "janneloffe@karlsson.se");
            //assert
            Assert.Same("janneloffe@karlsson.se", user.Email);


        }


        private async Task SeedData(UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
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


            //Roles
        
            var roles = new[] { "Customer", "Admin" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }


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


        public async Task InitializeAsync()
        {
            await _context.Database.EnsureCreatedAsync();
          
        }

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
        }
    }
}