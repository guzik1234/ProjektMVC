using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Services;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;

namespace ogloszenia.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILocalizationService _localizer;

        public AccountController(
            ApplicationDbContext context, 
            IAuthService authService,
            ILocalizationService localizer)
        {
            _context = context;
            _authService = authService;
            _localizer = localizer;
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id);
                }
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
                HttpContext.Session.SetString("Theme", user.Theme);
                HttpContext.Session.SetString("Language", user.Language);
                
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
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(int id, string firstName, string lastName, int pageSize, string theme, string language)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PageSize = pageSize;
            user.Theme = theme;
            user.Language = language;

            // Store theme in session for immediate effect
            HttpContext.Session.SetString("Theme", theme);
            HttpContext.Session.SetString("Language", language);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = _localizer.Get("ProfileUpdatedSuccessfully");
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
                TempData["SuccessMessage"] = _localizer.Get("PasswordChangedSuccessfully");
                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError("", message);
            return View();
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Podaj adres email");
                return View();
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("", "Podaj nowe hasło i potwierdź je");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Hasła nie są identyczne");
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Hasło musi mieć minimum 6 znaków");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Nie znaleziono użytkownika z tym adresem email");
                return View();
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hasło zostało zmienione. Możesz się teraz zalogować.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword(string token, int userId)
        {
            var storedToken = HttpContext.Session.GetString($"PasswordResetToken_{userId}");
            var expiryStr = HttpContext.Session.GetString($"PasswordResetExpiry_{userId}");

            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                TempData["ErrorMessage"] = _localizer.Get("InvalidResetToken");
                return RedirectToAction(nameof(Login));
            }

            if (!string.IsNullOrEmpty(expiryStr) && DateTime.Parse(expiryStr) < DateTime.Now)
            {
                TempData["ErrorMessage"] = _localizer.Get("ResetTokenExpired");
                return RedirectToAction(nameof(Login));
            }

            ViewBag.Token = token;
            ViewBag.UserId = userId;
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, string token, string newPassword, string confirmPassword)
        {
            var storedToken = HttpContext.Session.GetString($"PasswordResetToken_{userId}");
            
            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                ModelState.AddModelError("", "Nieprawidłowy token resetowania hasła.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Hasła się nie zgadzają");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Clear the reset token
            HttpContext.Session.Remove($"PasswordResetToken_{userId}");
            HttpContext.Session.Remove($"PasswordResetExpiry_{userId}");

            TempData["SuccessMessage"] = _localizer.Get("PasswordResetSuccessfully");
            return RedirectToAction(nameof(Login));
        }
    }
}
