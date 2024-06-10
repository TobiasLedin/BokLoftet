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
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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



        //REGISTER new user tests (Peter)

        //Acceptanskriterie:
        // - Verifiera att en ny post lagras i AspNetUsers-tabellen med samma email.
        [Fact]
        public async Task Register_AssertNewUserIsSavedToDatabase_EqualsTrue()
        {         
            //ARRANGE
            var newUserToSaveToDb = new RegisterViewModel()
            {

                FirstName = "Mario",
                LastName = "Lemieux",
                Email = "mario@lemiuex.com",
                Adress = "Pittsburgh Avenue 2",
                Phone = "412-642-PENS",
                Password = "Mario123!",
                ConfirmPassword = "Mario123!"
            };

            var controller = new AccountController(_userManager, _userStore, _signInManager);


            //ACT
            var result = await controller.RegisterAsync(newUserToSaveToDb);
            var user = await _userManager.FindByEmailAsync(newUserToSaveToDb.Email);


            //ASSERT

            //check that user is found in database by ensuring it's not null.
            Assert.NotNull(user);

            //check that the email of the user found in the database matches the email of the new user.
            Assert.Equal(newUserToSaveToDb.Email, user.Email);


            #region other code
            /*
            //ARRANGE
            var newUserToSaveToDb = new RegisterViewModel()
            {

                FirstName = "Mario",
                LastName = "Lemieux",
                Email = "mario@lemiuex.com",
                Adress = "Pittsburgh Avenue 2",
                Phone = "412-642-PENS",
                Password = "Mario123!",
                ConfirmPassword = "Mario123!"
            };

            //Mock HttpContext
            var mockHttpContext = A.Fake<HttpContext>();
            A.CallTo(() => mockHttpContext.Request.Method).Returns("POST");

            //Mock TempDataDictionaryFactory
            var mockTempDataDictionaryFactory = A.Fake<ITempDataDictionaryFactory>();
            var mockTempDataDictionary = A.Fake<ITempDataDictionary>();

            A.CallTo(() => mockTempDataDictionaryFactory.GetTempData(A<HttpContext>.Ignored)).Returns(mockTempDataDictionary);

            //Mock IUrlHelperFactory
            var mockUrlHelperFactory = A.Fake<IUrlHelperFactory>();

            #region code used earlier
         
            //var mockUrlHelper = A.Fake<IUrlHelper>();
            //A.CallTo(() => mockUrlHelperFactory.GetUrlHelper(A<ActionContext>.Ignored)).Returns(mockUrlHelper);
            #endregion


            //controller
            var controller = new AccountController(_userManager, _userStore, _signInManager);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext,

            };

            #region code used earlier
            //controller.TempData = mockTempDataDictionary;
            //controller.Url = mockUrlHelper;
            #endregion


            controller.ControllerContext.HttpContext.RequestServices = A.Fake<IServiceProvider>();

            A.CallTo(() => controller.ControllerContext.HttpContext.RequestServices.GetService(typeof(IUrlHelperFactory))).Returns(mockUrlHelperFactory);


            //ACT
            var result = await controller.RegisterAsync(newUserToSaveToDb);
            var user = await _userManager.FindByEmailAsync(newUserToSaveToDb.Email);

            //ASSERT

            //check that user is found in database by ensuring it's not null.
            Assert.NotNull(user);

            //check that the email of the user found in the database matches the email of the new user.
            Assert.Equal(newUserToSaveToDb.Email, user.Email);


            */
            #endregion other code
        }


        //Acceptanskriterie:
        //- Email-adress m�ste vara unik.
        [Fact]
        public async Task Register_AssertIfNewRegisteredUserEmail_AlreadyExistsInDatabase_EqualsTrue()
        {
           
            //ARRANGE
            var newUserWithExistingEmail = new RegisterViewModel()
            {

                FirstName = "Loffe2",
                LastName = "Karlsson2",
                Email = "janneloffe@karlsson.se",
                Adress = "Andra V�gen 2",
                Phone = "0771-123 455",
                Password = "Loffe123!",
                ConfirmPassword = "Loffe123!"
            };

            //create controller
            var controller = new AccountController(_userManager, _userStore, _signInManager);


            //ACT
            var result = await controller.RegisterAsync(newUserWithExistingEmail);


            //ASSERT

            //check that we get a view returned since that is what is returned when the register process fails.
            var viewResult = Assert.IsType<ViewResult>(result);

            //check if modelstate contains email key (of key-value pair).
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(nameof(newUserWithExistingEmail.Email)));

            //check if modelstate email key contains an error message stating e-mail already exists.
            Assert.Contains("E-mail already exists.", viewResult.ViewData.ModelState[nameof(newUserWithExistingEmail.Email)].Errors.Select(e => e.ErrorMessage));

        }



        //Acceptanskriterie:
        //- L�senord m�ste inneh�lla minst en stor bokstav, ett specialtecken och en siffra.*/
        [Fact]
        public async Task Register_AssertNewUserPasswordIncludes_CapitalLetter_Number_And_Symbol_EqualsTrue()
        {
                      
            //ARRANGE
            var newUserWithPasswordToCheck = new RegisterViewModel()
            {

                FirstName = "Jaromir",
                LastName = "Jagr",
                Email = "jagr@penguins.com",
                Adress = "Penguins Road 68",
                Phone = "0771-534 455",
                Password = "Jagr123!",
                ConfirmPassword = "Jagr123!"
            };

            //create controller
            var controller = new AccountController(_userManager, _userStore, _signInManager);


            //ACT
            var result = await controller.RegisterAsync(newUserWithPasswordToCheck);


            //ASSERT

            //run method to check if password contains capital letter, number and symbol.
            Assert.True(controller.CheckPassword(newUserWithPasswordToCheck.Password));

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
            Assert.False(book.IsAvailable, "Boken bör markeras som otillgänglig");
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
            Assert.True(book.IsAvailable, "Boken bör markeras som otillgänglig");
        }

    }
}