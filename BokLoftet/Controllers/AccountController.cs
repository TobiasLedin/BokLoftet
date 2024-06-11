using BokLoftet.Models;
using BokLoftet.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using System.ComponentModel.DataAnnotations;

namespace BokLoftet.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
        }



        //REGISTER
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel userData)
        {
            //check if password is correct format
            var passwordCorrectFormat = CheckPassword(userData.Password);

            //check if new users email exists in database
            var emailCheck = await _userManager.FindByEmailAsync(userData.Email);

            if (ModelState.IsValid)
            {
                if (passwordCorrectFormat == true)
                {
                    if (emailCheck == null)
                    //if no user is found, continue with registration
                    {
                        #region registration
                        var user = new ApplicationUser
                        {
                            FirstName = userData.FirstName,
                            LastName = userData.LastName,
                            Email = userData.Email,
                            Adress = userData.Adress,
                            PhoneNumber = userData.Phone,
                            NormalizedUserName = userData.Email.ToUpper(),
                            NormalizedEmail = userData.Email.ToUpper(),
                            EmailConfirmed = true
                        };

                        await _userStore.SetUserNameAsync(user, userData.Email, CancellationToken.None);
                        var result = await _userManager.CreateAsync(user, userData.Password);
                        await _userManager.AddToRoleAsync(user, "Customer");

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Login");

                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    #endregion

                    //if user with email already exist in database
                    else
                    {
                        //add error message to email key of modelstate
                        ModelState.AddModelError(nameof(userData.Email), "E-mail already exists.");
                    }
                }
                //if password format is incorrect
                else
                {
                    //add error message to password key of modelstate
                    ModelState.AddModelError(nameof(userData.Password), "Password must include at least one capital letter, one number, and one symbol.");
                }
            }

            return View();
        }



        //LOGIN
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginData)
        {
            if (ModelState.IsValid)
            {
                // Verify logged out state before logging in.
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                var result = await _signInManager.PasswordSignInAsync(loginData.Email, loginData.Password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Felaktiga inloggningsuppgifter!");
                }
            }
            return View();
        }

        //LOGOUT
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }


        //method to check if password is correct format
        public bool CheckPassword(string password)
        {
            bool hasCapitalLetter = false;
            bool hasDigit = false;
            bool hasSymbol = false;

            foreach (var character in password)
            {
                if (char.IsUpper(character))
                    hasCapitalLetter = true;
                else if (char.IsDigit(character))
                    hasDigit = true;
                else if (char.IsSymbol(character) || char.IsPunctuation(character))
                    hasSymbol = true;

            }

            return hasCapitalLetter && hasDigit && hasSymbol;
        }

    }
}
