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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task Login_IfLoginCredentialsValid_LoginUserAndRedirectToIndexPage()
        {
            // ARRANGE

            // Valid login credentials
            string email = "janneloffe@karlsson.se";
            string password = "Test123!";

            var loginCredentials = new LoginViewModel { Email = email, Password = password };

            // Mock HttpContext
            var fakeHttpContext = A.Fake<HttpContext>();


            // Mock IAuthenticationService
            var fakeAuthService = A.Fake<IAuthenticationService>();
            A.CallTo(() => fakeAuthService.SignInAsync(A<HttpContext>._, A<string>._, A<ClaimsPrincipal>._, A<AuthenticationProperties>._))
                .Returns(Task.CompletedTask);


            // Mock UrlHelper
            var fakeUrlHelper = A.Fake<IUrlHelper>();

            // Mock IUrlHelperFactory
            var fakeUrlHelperFactory = A.Fake<IUrlHelperFactory>();
            A.CallTo(() => fakeUrlHelperFactory.GetUrlHelper(A<ActionContext>._)).Returns(fakeUrlHelper);

            // Mock ITempDataDictionary
            var fakeTempDataDictionary = A.Fake<ITempDataDictionary>();

            //Mock ITempDataDictionaryFactory
            var fakeTempDataDictionaryFactory = A.Fake<ITempDataDictionaryFactory>();
            A.CallTo(() => fakeTempDataDictionaryFactory.GetTempData(fakeHttpContext)).Returns(fakeTempDataDictionary);
          

            // Add mocked services to a service collection
            // and use it with the mock HttpContext
            fakeHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(fakeAuthService)
                .AddSingleton<IUrlHelperFactory>(fakeUrlHelperFactory)
                .AddSingleton<ITempDataDictionaryFactory>(fakeTempDataDictionaryFactory)
                .BuildServiceProvider();

            // Provide SignInManager with mock HttpContext
            _signInManager.Context = fakeHttpContext;

            // Create controller instance and provide mock HttpContext
            var controller = new AccountController(_userManager, _userStore, _signInManager);

            controller.ControllerContext.HttpContext = fakeHttpContext;


            // ACT

            var result = await controller.Login(loginCredentials);


            // ASSERT

            Assert.True(controller.User.Identity.IsAuthenticated);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Login_IfLoginCredentialsInvalid_RemainOnLoginPageAndShowErrorMessage()
        {
            // ARRANGE

            // Invalid login credentials
            string email = "janneloffe@karlsson.se";
            string password = "wrongpassword";

            var loginCredentials = new LoginViewModel { Email = email, Password = password };

            // Mock HttpContext
            var fakeHttpContext = A.Fake<HttpContext>();

            // Mock IAuthenticationService
            var fakeAuthService = A.Fake<IAuthenticationService>();
            A.CallTo(() => fakeAuthService.SignInAsync(A<HttpContext>._, A<string>._, A<ClaimsPrincipal>._, A<AuthenticationProperties>._))
                .Returns(Task.CompletedTask);

            // Mock UrlHelper
            var fakeUrlHelper = A.Fake<IUrlHelper>();

            // Mock IUrlHelperFactory
            var fakeUrlHelperFactory = A.Fake<IUrlHelperFactory>();
            A.CallTo(() => fakeUrlHelperFactory.GetUrlHelper(A<ActionContext>._)).Returns(fakeUrlHelper);

            // Mock ITempDataDictionary
            var fakeTempDataDictionary = A.Fake<ITempDataDictionary>();

            //Mock ITempDataDictionaryFactory
            var fakeTempDataDictionaryFactory = A.Fake<ITempDataDictionaryFactory>();
            A.CallTo(() => fakeTempDataDictionaryFactory.GetTempData(fakeHttpContext)).Returns(fakeTempDataDictionary);

            // Add mock IAuthenticationService and mock IUrlHelperFactory service
            // to a service collection and use it with the mock HttpContext
            fakeHttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(fakeAuthService)
                .AddSingleton<IUrlHelperFactory>(fakeUrlHelperFactory)
                .AddSingleton<ITempDataDictionaryFactory>(fakeTempDataDictionaryFactory)
                .BuildServiceProvider();

            // Provide SignInManager with mock HttpContext
            _signInManager.Context = fakeHttpContext;

            // Create controller instance with mock HttpContext
            var controller = new AccountController(_userManager, _userStore, _signInManager);

            controller.ControllerContext.HttpContext = fakeHttpContext;


            // ACT

            var result = await controller.Login(loginCredentials);


            // ASSERT

            var error = controller.ModelState.Values.SelectMany(x => x.Errors).FirstOrDefault();

            Assert.Equal("Felaktiga inloggningsuppgifter!", error.ErrorMessage);

            Assert.False(controller.User.Identity.IsAuthenticated);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Loan_IfCustomerHasNoActiveLoanOrders_DisplayNoLoansMessage()
        {
            // Arrange
            var customer = "janneloffe@karlsson.se";
            string? message = null;


            // Act
            if (_context.Orders.Where(x => x.Customer.Email == customer).Any() is false) 
            {
                message = "Du har inga aktiva låneordrar";
            }


            // Assert
            Assert.NotNull(message);
        }

        [Fact]
        public async Task Loan_IfCustomerHasActiveLoanOrders_DisplayLoanOrders()
        {
            // Arrange
            var customer = "janneloffe@karlsson.se";

            var order = new Order
            {
                Customer = _context.Users.First(x => x.Email == customer),
                Books = new List<Book>
                {
                    _context.Books.First()
                }
            };

            await _context.Orders.AddAsync(order);
            _context.SaveChanges();


            // Act
            var orders = await _context.Orders.Where(x => x.Customer.Email == customer).ToListAsync();


            // Assert
            Assert.NotEmpty(orders);
            Assert.Equal(orders.First().Books.First().Title, "Pippi Långstrump");
        }

        [Fact]  // Test to verify that books with matching title are returned
        public void Search_MatchingTitle_ReturnsBooks()
        {
            //Arrange
            var searchString = "Pippi Långstrump";
            var controller = new BookController(_userManager, _userStore, _signInManager, _context);

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
            var controller = new BookController(_userManager, _userStore, _signInManager, _context);

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
            var controller = new BookController(_userManager, _userStore, _signInManager, _context);

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
            var controller = new BookController(_userManager, _userStore, _signInManager, _context);

            //Act
            var result = controller.Search(searchString) as ViewResult;

            // Assert: Verify that the result is not null and books with the correct category are returned

            Assert.NotNull(result);
            var books = result.Model as List<Book>;
            Assert.NotNull(books);
            Assert.Single(books);
            Assert.All(books, book => Assert.Equal(searchString, book.Category.Name));
        }

        //REGISTER new user tests (Peter)

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


        [Fact]
        public async Task Register_AssertIfNewRegisteredUserEmail_AlreadyExistsInDatabase_EqualsTrue()
        {
           
            //ARRANGE
            var newUserWithExistingEmail = new RegisterViewModel()
            {

                FirstName = "Loffe2",
                LastName = "Karlsson2",
                Email = "janneloffe@karlsson.se",
                Adress = "Andra Vägen 2",
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


        [Fact]
        public async Task Register_AssertInvalidPassword_ReturnsCorrectErrorMessage()
        {
                      
            //ARRANGE
            var newUserWithInvalidPassword = new RegisterViewModel()
            {

                FirstName = "Jaromir",
                LastName = "Jagr",
                Email = "jagr@penguins.com",
                Adress = "Penguins Road 68",
                Phone = "0771-534 455",
                Password = "jagr",                          //not a valid password
                ConfirmPassword = "Jagr123!"
            };

            //create controller
            var controller = new AccountController(_userManager, _userStore, _signInManager);


            //ACT
            var result = await controller.RegisterAsync(newUserWithInvalidPassword) as ViewResult;


            //ASSERT

          
            //check if modelstate contains password key.
            Assert.True(result.ViewData.ModelState.ContainsKey(nameof(newUserWithInvalidPassword.Password)));

            //check if modelstate password key contains an error message stating password format is incorrect.
            Assert.Contains("Password must include at least one capital letter, one number, and one symbol.", result.ViewData.ModelState[nameof(newUserWithInvalidPassword.Password)].Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task LoanBook()
        {
            //Arrange
            var book = _context.Books.FirstOrDefault();
            if (book != null)
            {
                book.IsAvailable = true;
                await _context.SaveChangesAsync();
            }

            var user = _context.Users.FirstOrDefault();
            var bookController = new BookController(_userManager, _userStore, _signInManager, _context);

            Assert.NotNull(book);
            Assert.NotNull(user);

            //Act
            await bookController.Loan(book.Id, user.Id);

            //Assert
            Assert.False(book.IsAvailable, "Boken bör markeras som otillgänglig");
            var order = _context.Orders.Include(o => o.Books).FirstOrDefault(o => o.Books.Any(b => b.Id == book.Id) && o.Customer.Id == user.Id);
            Assert.NotNull(order);
            Assert.Contains(book, order.Books);
            Assert.Equal(user.Id, order.Customer.Id);
        }

        [Fact]
        public async Task ReturnBook()
        {
            var book = _context.Books.FirstOrDefault(b => b.IsAvailable);
            if (book != null)
            {
                book.IsAvailable = false;
            }

            var bookController = new BookController(_userManager, _userStore, _signInManager, _context);
            Assert.NotNull(book);

            //Act
            await bookController.Return(book.Id);

            //Assert
            Assert.True(book.IsAvailable, "Boken bör markeras som tillgänglig");
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