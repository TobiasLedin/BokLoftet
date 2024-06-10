using BokLoftet.Controllers;
using BokLoftet.Data;
using BokLoftet.Models;
using BokLoftet.ViewModels;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserStore<ApplicationUser> _userStore;

        public DbTests()
        {
            var builder = new WebHostBuilder().UseStartup<DbTestStartup>();

            _server = new TestServer(builder);
            _serviceProvider = _server.Host.Services;

            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            _signInManager = _serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            _userStore = _serviceProvider.GetRequiredService<IUserStore<ApplicationUser>>();
        }


        [Fact]
        public void DB_CheckIfCategoryExists()
        {
            var category = _context.Categories.FirstOrDefault(x => x.Name == "Barnböcker");

            Assert.NotNull(category);

        }

        [Fact]
        public async Task Login_IfLoginCredentialsValid_AssertSignInManagerSucceededEqualTrue()
        {
            // Arrange

            // Valid login credentials
            string email = "janneloffe@karlsson.se";
            string password = "Test123!";

            var loginCredentials = new LoginViewModel { Email = email, Password = password };

            // Mock HttpContext
            var fakeHttpContext = A.Fake<HttpContext>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, email)
            }, "mock"));

            A.CallTo(() => fakeHttpContext.User).Returns(user);


            // Mock IAuthenticationService
            var authService = A.Fake<IAuthenticationService>();
            A.CallTo(() => authService.SignInAsync(A<HttpContext>._, A<string>._, A<ClaimsPrincipal>._, A<AuthenticationProperties>._))
                .Returns(Task.CompletedTask);

            // Add mock IAuthenticationService service to mock HttpContext
            fakeHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(authService)
                .BuildServiceProvider();

            _signInManager.Context = fakeHttpContext;


            // Act

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);


            // Assert

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Login_IfLoginCredentialsInvalid_AssertSignInManagerSucceededEqualFalse()
        {
            // Arrange

            // Invalid login credentials
            string email = "janneloffe@karlsson.se";
            string password = "fellösenord";

            var loginCredentials = new LoginViewModel { Email = email, Password = password };

            // Mock HttpContext
            var fakeHttpContext = A.Fake<HttpContext>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, email)
            }, "mock"));

            A.CallTo(() => fakeHttpContext.User).Returns(user);


            // Mock IAuthenticationService
            var authService = A.Fake<IAuthenticationService>();
            A.CallTo(() => authService.SignInAsync(A<HttpContext>._, A<string>._, A<ClaimsPrincipal>._, A<AuthenticationProperties>._))
                .Returns(Task.CompletedTask);

            fakeHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(authService)
                .BuildServiceProvider();

            _signInManager.Context = fakeHttpContext;


            // Act

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);


            // Assert

            Assert.False(result.Succeeded);
        }


        [Fact]  // Test to verify that books with matching title are returned
        public void Search_MatchingTitle_ReturnsBooks()
        {
            //Arrange
            var searchString = "Pippi Långstrump";
            var controller = new BookController(_context);

            //Act 
            var result = controller.Search(searchString) as ViewResult;

            //Assert  Verify that the result is not null and the correct book is returned
            Assert.NotNull(result);
            var books = result.Model as List<Book>;
            Assert.NotNull(books);
            Assert.Single(books);
            Assert.Equal(searchString, books[0].Title);
        }
        [Fact]     // Verify that a view for no results is returned if no matching title is found
        public void Search_NoResultFromSearch_ReturnsNoResultsView()
        {
            //Arrange
            var searchString = "Nonexistent Book";
            var controller = new BookController(_context);

            //Act
            var result = controller.Search(searchString) as ViewResult;

            // Assert: Verify that the result is not null and the "NoResults" view is returned
            Assert.NotNull(result);
            Assert.Equal("NoResults", result.ViewName);
        }
        [Fact]  // Verify that books with matching author are returned
        public void Search_MatchingAuthor_ReturnsBooks()
        {
            //Arrange
            var searchString = "Astrid Lindgren";
            var controller = new BookController(_context);

            //Act
            var result = controller.Search(searchString) as ViewResult;

            // Assert: Verify that the result is not null and books with the correct author are returned
            Assert.NotNull(result);
            var books = result.Model as List<Book>;
            Assert.NotNull(books);
            Assert.All(books, book => Assert.Equal(searchString, book.Author));
        }
        [Fact]     // Test to verify that books with matching category are returned

        public void Search_MatchingCategory_ReturnsBooks()
        {
            //Arrange
            var searchString = "Thriller";
            var controller = new BookController(_context);

            //Act
            var result = controller.Search(searchString) as ViewResult;

            // Assert: Verify that the result is not null and books with the correct category are returned

            Assert.NotNull(result);
            var books = result.Model as List<Book>;
            Assert.NotNull(books);
            Assert.Single(books);
            Assert.All(books, book => Assert.Equal(searchString, book.Category.Name));
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
                new Category { Name = "Barnböcker" },
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
                    Title = "Pippi Långstrump",
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
                    Author = "Lee Child",
                    Category = categories[1],
                    Title = "Jack Reacher",
                    Description = "En festlig bok om en stark stor kille.",
                    Language = "Engelska",
                    Publisher = "Bantam Books",
                    PublishYear = 1997,
                    Pages = 576,
                    ISBN = "9780515153651",
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
                Adress = "Blomvägen 1, Göteborg",
                Email = "janneloffe@karlsson.se",
                NormalizedEmail = "JANNELOFFE@KARLSSON.SE",
                PhoneNumber = "555 123 456",
                UserName = "janneloffe@karlsson.se"
            };

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

            await _userManager.CreateAsync(user1, "Test123!");
            await _userManager.AddToRoleAsync(user1, "Customer");

            await _userManager.CreateAsync(user2, "Test123!");
            await _userManager.AddToRoleAsync(user2, "Admin");

            _context.SaveChanges();
        }
    }
}