using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Services;

namespace ogloszenia.Controllers
{
    public class AccountController : Controller
    {
        private readonly List<User> _users;
        private readonly IAuthService _authService;

        public AccountController()
        {
            _users = InMemoryDatabase.Users;
            _authService = new AuthService(_users);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string firstName, string lastName)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Hasła się nie zgadzają");
                return View();
            }

            var (success, message) = await _authService.RegisterAsync(username, email, password, firstName, lastName);
            
            if (success)
            {
                HttpContext.Session.SetInt32("UserId", _users.FirstOrDefault(u => u.Username == username)?.Id ?? 0);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", message);
            return View();
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var (success, user) = await _authService.LoginAsync(username, password);
            
            if (success && user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("IsAdmin", user.IsAdmin ? 1 : 0);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Niepoprawne dane logowania");
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(int id, string firstName, string lastName, int pageSize, string theme, string language)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != id)
            {
                return Forbid();
            }

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PageSize = pageSize;
            user.Theme = theme;
            user.Language = language;

            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/ChangePassword
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Nowe hasła się nie zgadzają");
                return View();
            }

            var (success, message) = await _authService.ChangePasswordAsync(userId.Value, oldPassword, newPassword);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Hasło zmienione pomyślnie";
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError("", message);
            return View();
        }
    }
}
