using BokLoftet.Controllers;
using BokLoftet.Data;
using BokLoftet.Models;
using BokLoftet.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DbTests()
        {
            var builder = new WebHostBuilder().UseStartup<DbTestStartup>();

            _server = new TestServer(builder);
            _serviceProvider = _server.Host.Services;

            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
     
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
    }
}