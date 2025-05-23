using BCrypt.Net;
using Csharp.Data;
using Csharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

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
            // Posíláme pevně typovaný ViewModel
            return View(new AccountViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                // Pošleme do View ViewModel s modelem pro registraci a prázdný Login
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

            // Můžeš uživatele rovnou přihlásit, tady jen redirect
            return RedirectToAction("Index", "Account");
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

            // Přihlášení uživatele (cookie apod.) - není zde řešeno
            // await SignInUser(user);

            return RedirectToAction("Notes", "Home");
        }
    }
}
