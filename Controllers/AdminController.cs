using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Data;
using ogloszenia.Services;
using Microsoft.EntityFrameworkCore;

namespace ogloszenia.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILocalizationService _localizer;

        public AdminController(ApplicationDbContext context, ILocalizationService localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        private async Task<bool> IsAdminAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.IsAdmin ?? false;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalAdvertisements = await _context.Advertisements.CountAsync();
            ViewBag.PendingModeration = await _context.ModerationReports.CountAsync(r => r.Status == ModerationReportStatus.Pending);
            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var users = await _context.Users
                .OrderBy(u => u.Username)
                .ToListAsync();

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
        public async Task<IActionResult> ToggleAdmin(int userId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            user.IsAdmin = !user.IsAdmin;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = _localizer.Get("UserStatusUpdated");
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeactivateUser
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = _localizer.Get("UserStatusUpdated");
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var rootCategories = await _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
            return View(rootCategories);
        }

        // GET: /Admin/CreateCategory
        public async Task<IActionResult> CreateCategory(int? parentId)
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            ViewBag.ParentCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
            ViewBag.ParentId = parentId;
            return View();
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name, string description, int? parentCategoryId)
        {
            if (!await IsAdminAsync()) return Forbid();

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = _localizer.Get("CategoryNameRequired");
                return RedirectToAction(nameof(CreateCategory));
            }

            var category = new Category
            {
                Name = name,
                Description = description,
                ParentCategoryId = parentCategoryId,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = _localizer.Get("CategoryCreated");
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/EditCategory
        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, string name, string description)
        {
            if (!await IsAdminAsync()) return Forbid();

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Kategoria nie znaleziona";
                return RedirectToAction(nameof(Categories));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Nazwa kategorii jest wymagana";
                return RedirectToAction(nameof(Categories));
            }

            category.Name = name;
            category.Description = description;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kategoria zaktualizowana";
            return RedirectToAction(nameof(Categories));
        }

        // POST: /Admin/DeleteCategory
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!await IsAdminAsync()) return Forbid();

            var category = await _context.Categories
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Kategoria nie znaleziona";
                return RedirectToAction(nameof(Categories));
            }

            if (category.ChildCategories != null && category.ChildCategories.Count > 0)
            {
                TempData["ErrorMessage"] = "Nie można usunąć kategorii z podkategoriami";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kategoria usunięta";
            return RedirectToAction(nameof(Categories));
        }

        // GET: /Admin/ForbiddenWords
        public async Task<IActionResult> ForbiddenWords()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            ViewBag.ForbiddenWords = settings?.ForbiddenWords ?? new List<string>();
            return View();
        }

        // POST: /Admin/AddForbiddenWord
        [HttpPost]
        public async Task<IActionResult> AddForbiddenWord(string word)
        {
            if (!await IsAdminAsync()) return Forbid();

            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["ErrorMessage"] = "Słowo nie może być puste";
                return RedirectToAction(nameof(ForbiddenWords));
            }

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }

            if (settings.ForbiddenWords == null)
            {
                settings.ForbiddenWords = new List<string>();
            }

            if (!settings.ForbiddenWords.Contains(word.ToLower()))
            {
                settings.ForbiddenWords.Add(word.ToLower());
                settings.LastUpdated = DateTime.Now;
                _context.Entry(settings).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dodano słowo: {word}";
            }
            else
            {
                TempData["ErrorMessage"] = "To słowo już istnieje na liście";
            }

            return RedirectToAction(nameof(ForbiddenWords));
        }

        // POST: /Admin/RemoveForbiddenWord
        [HttpPost]
        public async Task<IActionResult> RemoveForbiddenWord(string word)
        {
            if (!await IsAdminAsync()) return Forbid();

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings != null && settings.ForbiddenWords != null)
            {
                settings.ForbiddenWords.Remove(word);
                settings.LastUpdated = DateTime.Now;
                _context.Entry(settings).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Usunięto słowo: {word}";
            }

            return RedirectToAction(nameof(ForbiddenWords));
        }

        // GET: /Admin/Settings
        public async Task<IActionResult> Settings()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings
                {
                    AdminMessage = "",
                    LastUpdated = DateTime.Now
                };
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        // POST: /Admin/Settings
        [HttpPost]
        public async Task<IActionResult> Settings(
            string adminMessage, 
            int maxFileSizeMB, 
            int maxFilesPerAdvertisement, 
            int maxMediaPerAdvertisement)
        {
            if (!await IsAdminAsync()) return Forbid();

            var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (existingSettings == null)
            {
                existingSettings = new SystemSettings();
                _context.SystemSettings.Add(existingSettings);
            }

            existingSettings.AdminMessage = adminMessage ?? string.Empty;
            existingSettings.MaxFileSize = maxFileSizeMB * 1024 * 1024; // Convert MB to bytes
            existingSettings.MaxFilesPerAdvertisement = maxFilesPerAdvertisement;
            existingSettings.MaxMediaPerAdvertisement = maxMediaPerAdvertisement;
            existingSettings.LastUpdated = DateTime.Now;
            
            _context.Entry(existingSettings).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = _localizer.Get("SettingsUpdated");
            return RedirectToAction(nameof(Settings));
        }

        // GET: /Admin/Moderation
        public async Task<IActionResult> Moderation(int page = 1, int pageSize = 10)
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var reports = await _context.ModerationReports
                .Include(r => r.Advertisement)
                .Include(r => r.ReportedByUser)
                .Where(r => r.Status == ModerationReportStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

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
        public async Task<IActionResult> ApproveReport(int reportId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var report = await _context.ModerationReports
                .Include(r => r.Advertisement)
                .FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return NotFound();

            report.Status = ModerationReportStatus.Approved;
            report.ResolvedAt = DateTime.Now;

            // Delete the advertisement
            if (report.Advertisement != null)
            {
                _context.Advertisements.Remove(report.Advertisement);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = _localizer.Get("ReportApproved");
            return RedirectToAction(nameof(Moderation));
        }

        // POST: /Admin/RejectReport
        [HttpPost]
        public async Task<IActionResult> RejectReport(int reportId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var report = await _context.ModerationReports.FirstOrDefaultAsync(r => r.Id == reportId);
            if (report == null) return NotFound();

            report.Status = ModerationReportStatus.Rejected;
            report.ResolvedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = _localizer.Get("ReportRejected");
            return RedirectToAction(nameof(Moderation));
        }

        // GET: /Admin/AdminMessage
        public async Task<IActionResult> AdminMessage()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            return View(settings);
        }

        // POST: /Admin/AdminMessage
        [HttpPost]
        public async Task<IActionResult> AdminMessage(string message)
        {
            if (!await IsAdminAsync()) return Forbid();

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
            }

            settings.AdminMessage = message ?? string.Empty;
            settings.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = _localizer.Get("MessageUpdated");
            return RedirectToAction(nameof(AdminMessage));
        }

        // GET: /Admin/Dictionaries
        public async Task<IActionResult> Dictionaries()
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var dictionaries = await _context.Dictionaries
                .Include(d => d.Values)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(dictionaries);
        }

        // POST: /Admin/CreateDictionary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDictionary(string name, string description)
        {
            if (!await IsAdminAsync()) return Forbid();

            var dictionary = new Dictionary
            {
                Name = name,
                Description = description ?? string.Empty
            };

            _context.Dictionaries.Add(dictionary);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Słownik został utworzony";
            return RedirectToAction(nameof(Dictionaries));
        }

        // POST: /Admin/DeleteDictionary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDictionary(int id)
        {
            if (!await IsAdminAsync()) return Forbid();

            var dictionary = await _context.Dictionaries
                .Include(d => d.Values)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dictionary != null)
            {
                _context.Dictionaries.Remove(dictionary);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Słownik został usunięty";
            }

            return RedirectToAction(nameof(Dictionaries));
        }

        // POST: /Admin/AddDictionaryValue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDictionaryValue(int dictionaryId, string value)
        {
            if (!await IsAdminAsync()) return Forbid();

            var dictionaryValue = new DictionaryValue
            {
                DictionaryId = dictionaryId,
                Value = value
            };

            _context.DictionaryValues.Add(dictionaryValue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wartość została dodana";
            return RedirectToAction(nameof(Dictionaries));
        }

        // POST: /Admin/DeleteDictionaryValue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDictionaryValue(int id)
        {
            if (!await IsAdminAsync()) return Forbid();

            var value = await _context.DictionaryValues.FindAsync(id);
            if (value != null)
            {
                _context.DictionaryValues.Remove(value);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Wartość została usunięta";
            }

            return RedirectToAction(nameof(Dictionaries));
        }

        // GET: /Admin/Attributes?categoryId=X
        public async Task<IActionResult> Attributes(int categoryId)
        {
            if (!await IsAdminAsync()) return RedirectToAction("Login", "Account");

            var category = await _context.Categories
                .Include(c => c.Attributes)
                .ThenInclude(a => a.Dictionary)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null) return NotFound();

            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category.Name;
            ViewBag.Dictionaries = await _context.Dictionaries.ToListAsync();

            return View(category.Attributes);
        }

        // POST: /Admin/CreateAttribute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAttribute(int categoryId, string name, string description, 
            string attributeType, bool isRequired, int? dictionaryId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var attribute = new AdvertisementAttribute
            {
                CategoryId = categoryId,
                Name = name,
                Description = description ?? string.Empty,
                AttributeType = attributeType,
                IsRequired = isRequired,
                DictionaryId = attributeType == "dictionary" ? dictionaryId : null
            };

            _context.AdvertisementAttributes.Add(attribute);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Atrybut został utworzony";
            return RedirectToAction(nameof(Attributes), new { categoryId });
        }

        // POST: /Admin/DeleteAttribute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttribute(int id, int categoryId)
        {
            if (!await IsAdminAsync()) return Forbid();

            var attribute = await _context.AdvertisementAttributes.FindAsync(id);
            if (attribute != null)
            {
                _context.AdvertisementAttributes.Remove(attribute);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Atrybut został usunięty";
            }

            return RedirectToAction(nameof(Attributes), new { categoryId });
        }
    }
}
