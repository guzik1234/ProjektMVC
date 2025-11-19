using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Services;

namespace ogloszenia.Controllers
{
    public class AdminController : Controller
    {
        private readonly List<User> _users;
        private readonly List<Category> _categories;
        private readonly List<Advertisement> _advertisements;
        private readonly List<ModerationReport> _reports;
        private readonly SystemSettings _settings;

        public AdminController()
        {
            _users = InMemoryDatabase.Users;
            _categories = InMemoryDatabase.Categories;
            _advertisements = InMemoryDatabase.Advertisements;
            _reports = InMemoryDatabase.ModerationReports;
            _settings = InMemoryDatabase.SystemSettings;
        }

        private bool IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;
            var user = _users.FirstOrDefault(u => u.Id == userId);
            return user?.IsAdmin ?? false;
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = _users.Count;
            ViewBag.TotalAdvertisements = _advertisements.Count;
            ViewBag.PendingModeration = _reports.Count(r => r.Status == ModerationReportStatus.Pending);
            ViewBag.TotalCategories = _categories.Count;

            return View();
        }

        // GET: /Admin/Users
        public IActionResult Users(int page = 1, int pageSize = 10)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var users = _users
                .OrderBy(u => u.CreatedAt)
                .ToList();

            int totalPages = (int)Math.Ceiling(users.Count / (double)pageSize);
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedUsers);
        }

        // POST: /Admin/ToggleAdmin
        [HttpPost]
        public IActionResult ToggleAdmin(int userId)
        {
            if (!IsAdmin()) return Forbid();

            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            user.IsAdmin = !user.IsAdmin;
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeactivateUser
        [HttpPost]
        public IActionResult DeactivateUser(int userId)
        {
            if (!IsAdmin()) return Forbid();

            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Categories
        public IActionResult Categories()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var rootCategories = _categories.Where(c => c.ParentCategoryId == null).ToList();
            return View(rootCategories);
        }

        // GET: /Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.ParentCategories = _categories.Where(c => c.ParentCategoryId == null).ToList();
            return View();
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        public IActionResult CreateCategory(string name, string description, int? parentCategoryId)
        {
            if (!IsAdmin()) return Forbid();

            var category = new Category
            {
                Id = InMemoryDatabase.GetNextCategoryId(),
                Name = name,
                Description = description,
                ParentCategoryId = parentCategoryId,
                CreatedAt = DateTime.Now
            };

            if (parentCategoryId.HasValue)
            {
                var parent = _categories.FirstOrDefault(c => c.Id == parentCategoryId);
                if (parent != null)
                {
                    category.ParentCategory = parent;
                }
            }

            _categories.Add(category);
            return RedirectToAction(nameof(Categories));
        }

        // GET: /Admin/ForbiddenWords
        public IActionResult ForbiddenWords()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.ForbiddenWords = _settings.ForbiddenWords;
            return View();
        }

        // POST: /Admin/AddForbiddenWord
        [HttpPost]
        public IActionResult AddForbiddenWord(string word)
        {
            if (!IsAdmin()) return Forbid();

            if (!string.IsNullOrWhiteSpace(word) && !_settings.ForbiddenWords.Contains(word.ToLower()))
            {
                _settings.ForbiddenWords.Add(word.ToLower());
            }

            return RedirectToAction(nameof(ForbiddenWords));
        }

        // POST: /Admin/RemoveForbiddenWord
        [HttpPost]
        public IActionResult RemoveForbiddenWord(string word)
        {
            if (!IsAdmin()) return Forbid();

            _settings.ForbiddenWords.Remove(word);
            return RedirectToAction(nameof(ForbiddenWords));
        }

        // GET: /Admin/Settings
        public IActionResult Settings()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            return View(_settings);
        }

        // POST: /Admin/Settings
        [HttpPost]
        public IActionResult Settings(SystemSettings settings)
        {
            if (!IsAdmin()) return Forbid();

            _settings.AdminMessage = settings.AdminMessage;
            _settings.LastUpdated = DateTime.Now;

            return RedirectToAction(nameof(Settings));
        }

        // GET: /Admin/Moderation
        public IActionResult Moderation(int page = 1, int pageSize = 10)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var reports = _reports
                .Where(r => r.Status == ModerationReportStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            int totalPages = (int)Math.Ceiling(reports.Count / (double)pageSize);
            var pagedReports = reports
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedReports);
        }

        // POST: /Admin/ApproveReport
        [HttpPost]
        public IActionResult ApproveReport(int reportId)
        {
            if (!IsAdmin()) return Forbid();

            var report = _reports.FirstOrDefault(r => r.Id == reportId);
            if (report == null) return NotFound();

            report.Status = ModerationReportStatus.Approved;
            report.ResolvedAt = DateTime.Now;

            // Delete the advertisement
            var ad = _advertisements.FirstOrDefault(a => a.Id == report.AdvertisementId);
            if (ad != null)
            {
                _advertisements.Remove(ad);
            }

            return RedirectToAction(nameof(Moderation));
        }

        // POST: /Admin/RejectReport
        [HttpPost]
        public IActionResult RejectReport(int reportId)
        {
            if (!IsAdmin()) return Forbid();

            var report = _reports.FirstOrDefault(r => r.Id == reportId);
            if (report == null) return NotFound();

            report.Status = ModerationReportStatus.Rejected;
            report.ResolvedAt = DateTime.Now;

            return RedirectToAction(nameof(Moderation));
        }
    }
}
