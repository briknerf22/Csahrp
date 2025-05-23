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
        public IActionResult Index()
        {
            return View(new AccountViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new AccountViewModel { Register = model, Login = new Login() });
            }

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Register.Username", "Username already taken.");
                return View("Index", new AccountViewModel { Register = model, Login = new Login() });
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ConsentForAITraining = model.AiTrainingConsent
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Přihlášení uživatele po registraci
            await SignInUser(user);

            return RedirectToAction("Index", "Notes");
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new AccountViewModel { Register = new Register(), Login = model });
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("Login.Username", "Invalid username or password.");
                return View("Index", new AccountViewModel { Register = new Register(), Login = model });
            }

            // Přihlášení uživatele po úspěšném přihlášení
            await SignInUser(user);

            return RedirectToAction("Index", "Notes");
        }

        // Metoda pro cookie autentizaci uživatele
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Account");
        }
        [Authorize]
        [HttpGet]
        public IActionResult DeleteAccount()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteAccount(string password)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var user = await _db.Users.Include(u => u.Notes).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Nesprávné heslo.");
                return View();
            }

            _db.Notes.RemoveRange(user.Notes);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Account");
        }
    }
}