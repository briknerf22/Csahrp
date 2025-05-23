using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Csharp.Data;
using Csharp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Csharp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Register model)
        {
            if (!model.AiTrainingConsent)
            {
                ModelState.AddModelError("AiTrainingConsent", "You must accept the AI training consent.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ConsentForAITraining = model.AiTrainingConsent
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Po registraci neprovádíme automatické přihlášení, ale přesměrujeme na login
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            await SignInUser(user);

            return RedirectToAction("Index", "Notes");
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
