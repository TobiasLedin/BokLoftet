using BokLoftet.Models;
using BokLoftet.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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


        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel userData)
        {
            if (ModelState.IsValid)
            {
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

            return View();
        }


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
            }
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }


    }
}
