using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using ogloszenia.Models;
using ogloszenia.Data;
using ogloszenia.Services;
using Microsoft.EntityFrameworkCore;

namespace ogloszenia.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISearchService _searchService;
        private readonly IModerationService _moderationService;
        private readonly IRssService _rssService;

        public AdvertisementController(
            ApplicationDbContext context,
            ISearchService searchService,
            IModerationService moderationService,
            IRssService rssService)
        {
            _context = context;
            _searchService = searchService;
            _moderationService = moderationService;
            _rssService = rssService;
        }

        // GET: /Advertisement/Index
        public IActionResult Index(int page = 1, int pageSize = 10, int? userId = null)
        {
            // Debug: Log the userId parameter
            Console.WriteLine($"[AdvertisementController.Index] userId parameter: {(userId.HasValue ? userId.Value.ToString() : "NULL")}");
            
            var query = _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Where(a => a.Status == AdvertisementStatus.Active);

            // Filter by userId if provided
            if (userId.HasValue)
            {
                Console.WriteLine($"[AdvertisementController.Index] Filtering by userId: {userId.Value}");
                query = query.Where(a => a.UserId == userId.Value);
            }

            var ads = query.OrderByDescending(a => a.CreatedAt).ToList();
            Console.WriteLine($"[AdvertisementController.Index] Found {ads.Count} advertisements");

            int totalPages = (int)Math.Ceiling(ads.Count / (double)pageSize);
            var pagedAds = ads
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.UserId = userId;

            return View(pagedAds);
        }

        // GET: /Advertisement/Details/5
        public IActionResult Details(int id)
        {
            var ad = _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Include(a => a.Files)
                .Include(a => a.AttributeValues)
                .ThenInclude(av => av.Attribute)
                .FirstOrDefault(a => a.Id == id);
            
            if (ad == null)
            {
                return NotFound();
            }

            // Load dictionary values separately for each attribute value
            foreach (var attrVal in ad.AttributeValues)
            {
                if (attrVal.Attribute != null && attrVal.Attribute.AttributeType == "dictionary")
                {
                    attrVal.Attribute.Dictionary = _context.Dictionaries
                        .Include(d => d.Values)
                        .FirstOrDefault(d => d.Id == attrVal.Attribute.DictionaryId);
                }
            }

            // Increment view count
            ad.ViewCount++;
            _context.SaveChanges();

            return View(ad);
        }

        // GET: /Advertisement/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = _context.Categories
                .Include(c => c.ChildCategories)
                .ThenInclude(c => c.Attributes)
                .ThenInclude(a => a.Dictionary)
                .ThenInclude(d => d.Values)
                .Include(c => c.Attributes)
                .ThenInclude(a => a.Dictionary)
                .ThenInclude(d => d.Values)
                .Where(c => c.ParentCategoryId == null)
                .ToList();
            
            var settings = _context.SystemSettings.FirstOrDefault() ?? new SystemSettings();
            ViewBag.MaxFileSize = settings.MaxFileSize / (1024 * 1024);
            ViewBag.MaxMedia = settings.MaxMediaPerAdvertisement;
            ViewBag.MaxFiles = settings.MaxFilesPerAdvertisement;
            
            return View();
        }

        // POST: /Advertisement/Create
        [HttpPost]
        public async Task<IActionResult> Create(Advertisement advertisement, int[] selectedCategories, IFormFile[]? mediaFiles, IFormFile[]? attachmentFiles)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate for forbidden words
            var (isValid, forbiddenWords) = await _moderationService.ValidateContentAsync(
                $"{advertisement.Title} {advertisement.Description} {advertisement.DetailedDescription}");
            
            if (!isValid)
            {
                ModelState.AddModelError("", $"Ogłoszenie zawiera zabronione słowa: {string.Join(", ", forbiddenWords)}");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                return View(advertisement);
            }

            // Sanitize HTML in detailed description
            advertisement.DetailedDescription = _moderationService.SanitizeHtml(advertisement.DetailedDescription);

            // Get system settings for validation
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings();
            }

            // Validate file uploads against system settings
            if (mediaFiles != null && mediaFiles.Length > settings.MaxMediaPerAdvertisement)
            {
                ModelState.AddModelError("", $"Możesz dodać maksymalnie {settings.MaxMediaPerAdvertisement} zdjęć");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                return View(advertisement);
            }

            if (attachmentFiles != null && attachmentFiles.Length > settings.MaxFilesPerAdvertisement)
            {
                ModelState.AddModelError("", $"Możesz dodać maksymalnie {settings.MaxFilesPerAdvertisement} plików załączników");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                return View(advertisement);
            }

            // Check individual file sizes
            if (mediaFiles != null)
            {
                foreach (var file in mediaFiles)
                {
                    if (file != null && file.Length > settings.MaxFileSize)
                    {
                        ModelState.AddModelError("", $"Plik {file.FileName} jest za duży. Maksymalny rozmiar: {settings.MaxFileSize / (1024 * 1024)} MB");
                        ViewBag.Categories = _context.Categories
                            .Include(c => c.ChildCategories)
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
                        return View(advertisement);
                    }
                }
            }

            if (attachmentFiles != null)
            {
                foreach (var file in attachmentFiles)
                {
                    if (file != null && file.Length > settings.MaxFileSize)
                    {
                        ModelState.AddModelError("", $"Plik {file.FileName} jest za duży. Maksymalny rozmiar: {settings.MaxFileSize / (1024 * 1024)} MB");
                        ViewBag.Categories = _context.Categories
                            .Include(c => c.ChildCategories)
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
                        return View(advertisement);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                return View(advertisement);
            }

            advertisement.UserId = userId.Value;
            advertisement.CreatedAt = DateTime.Now;
            advertisement.Status = AdvertisementStatus.Active;
            advertisement.ViewCount = 0;

            // Add selected categories
            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                foreach (var catId in selectedCategories)
                {
                    var category = _context.Categories.FirstOrDefault(c => c.Id == catId);
                    if (category != null)
                    {
                        advertisement.Categories.Add(category);
                    }
                }
            }

            _context.Advertisements.Add(advertisement);
            _context.SaveChanges();

            // Save attribute values
            var attributeKeys = Request.Form.Keys.Where(k => k.StartsWith("attr_"));
            foreach (var key in attributeKeys)
            {
                var attrId = int.Parse(key.Replace("attr_", ""));
                var value = Request.Form[key].ToString();
                
                if (!string.IsNullOrEmpty(value))
                {
                    var attribute = _context.AdvertisementAttributes.Find(attrId);
                    var attributeValue = new AdvertisementAttributeValue
                    {
                        AdvertisementId = advertisement.Id,
                        AttributeId = attrId,
                        Value = value
                    };

                    // For dictionary type, also save DictionaryValueId
                    if (attribute?.AttributeType == "dictionary" && int.TryParse(value, out int dictValueId))
                    {
                        attributeValue.DictionaryValueId = dictValueId;
                        var dictValue = _context.DictionaryValues.Find(dictValueId);
                        attributeValue.Value = dictValue?.Value ?? value;
                    }

                    _context.AttributeValues.Add(attributeValue);
                }
            }
            _context.SaveChanges();

            // Save uploaded media files
            try
            {
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ads", advertisement.Id.ToString());
                Directory.CreateDirectory(uploadsRoot);

                if (mediaFiles != null && mediaFiles.Length > 0)
                {
                    foreach (var file in mediaFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName);
                            var savedName = Guid.NewGuid().ToString() + ext;
                            var savePath = Path.Combine(uploadsRoot, savedName);
                            using (var stream = new FileStream(savePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            var media = new AdvertisementMedia
                            {
                                AdvertisementId = advertisement.Id,
                                FileName = file.FileName,
                                FilePath = $"/uploads/ads/{advertisement.Id}/{savedName}",
                                MediaType = file.ContentType ?? "application/octet-stream",
                                UploadedAt = DateTime.Now
                            };
                            _context.Media.Add(media);
                        }
                    }
                }

                if (attachmentFiles != null && attachmentFiles.Length > 0)
                {
                    foreach (var file in attachmentFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName);
                            var savedName = Guid.NewGuid().ToString() + ext;
                            var savePath = Path.Combine(uploadsRoot, savedName);
                            using (var stream = new FileStream(savePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            var attachment = new AdvertisementFile
                            {
                                AdvertisementId = advertisement.Id,
                                FileName = file.FileName,
                                FilePath = $"/uploads/ads/{advertisement.Id}/{savedName}",
                                FileSize = file.Length,
                                UploadedAt = DateTime.Now
                            };
                            _context.Files.Add(attachment);
                        }
                    }
                }

                _context.SaveChanges();
            }
            catch
            {
                // If saving files fails, continue
            }

            // Update RSS feed
            await _rssService.UpdateRssFeedAsync();

            return RedirectToAction(nameof(Details), new { id = advertisement.Id });
        }

        // GET: /Advertisement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ad = _context.Advertisements
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Include(a => a.Files)
                .FirstOrDefault(a => a.Id == id);
                
            if (ad == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            
            if (userId == null || (ad.UserId != userId.Value && !isAdmin))
            {
                return Forbid();
            }

            ViewBag.Categories = _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToList();
            ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
            
            var settings = _context.SystemSettings.FirstOrDefault() ?? new SystemSettings();
            ViewBag.MaxFileSize = settings.MaxFileSize / (1024 * 1024);
            ViewBag.MaxMedia = settings.MaxMediaPerAdvertisement;
            ViewBag.MaxFiles = settings.MaxFilesPerAdvertisement;

            return View(ad);
        }

        // POST: /Advertisement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Advertisement advertisement, int[] selectedCategories, IFormFile[]? mediaFiles, IFormFile[]? attachmentFiles, int[]? mediaToDelete)
        {
            var ad = _context.Advertisements
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Include(a => a.Files)
                .FirstOrDefault(a => a.Id == id);
                
            if (ad == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            
            if (userId == null || (ad.UserId != userId.Value && !isAdmin))
            {
                return Forbid();
            }

            // Validate for forbidden words
            var (isValid, forbiddenWords) = _moderationService.ValidateContentAsync(
                $"{advertisement.Title} {advertisement.Description} {advertisement.DetailedDescription}").Result;
            
            if (!isValid)
            {
                ModelState.AddModelError("", $"Ogłoszenie zawiera zabronione słowa: {string.Join(", ", forbiddenWords)}");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                return View(ad);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                return View(ad);
            }

            // Get system settings for validation
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings();
            }

            // Validate new file uploads against system settings
            int currentMediaCount = ad.Media.Count;
            int currentFilesCount = ad.Files.Count;
            
            // Subtract deleted media if any
            if (mediaToDelete != null && mediaToDelete.Length > 0)
            {
                currentMediaCount -= mediaToDelete.Length;
            }

            int newMediaCount = (mediaFiles != null ? mediaFiles.Length : 0);
            int newFilesCount = (attachmentFiles != null ? attachmentFiles.Length : 0);

            if (currentMediaCount + newMediaCount > settings.MaxMediaPerAdvertisement)
            {
                ModelState.AddModelError("", $"Przekroczono limit zdjęć. Maksymalnie: {settings.MaxMediaPerAdvertisement} (masz: {currentMediaCount}, próbujesz dodać: {newMediaCount})");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                return View(ad);
            }

            if (currentFilesCount + newFilesCount > settings.MaxFilesPerAdvertisement)
            {
                ModelState.AddModelError("", $"Przekroczono limit plików. Maksymalnie: {settings.MaxFilesPerAdvertisement} (masz: {currentFilesCount}, próbujesz dodać: {newFilesCount})");
                ViewBag.Categories = _context.Categories
                    .Include(c => c.ChildCategories)
                    .Where(c => c.ParentCategoryId == null)
                    .ToList();
                ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                return View(ad);
            }

            // Check individual file sizes
            if (mediaFiles != null)
            {
                foreach (var file in mediaFiles)
                {
                    if (file != null && file.Length > settings.MaxFileSize)
                    {
                        ModelState.AddModelError("", $"Plik {file.FileName} jest za duży. Maksymalny rozmiar: {settings.MaxFileSize / (1024 * 1024)} MB");
                        ViewBag.Categories = _context.Categories
                            .Include(c => c.ChildCategories)
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
                        ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                        return View(ad);
                    }
                }
            }

            if (attachmentFiles != null)
            {
                foreach (var file in attachmentFiles)
                {
                    if (file != null && file.Length > settings.MaxFileSize)
                    {
                        ModelState.AddModelError("", $"Plik {file.FileName} jest za duży. Maksymalny rozmiar: {settings.MaxFileSize / (1024 * 1024)} MB");
                        ViewBag.Categories = _context.Categories
                            .Include(c => c.ChildCategories)
                            .Where(c => c.ParentCategoryId == null)
                            .ToList();
                        ViewBag.SelectedCategoryIds = ad.Categories.Select(c => c.Id).ToArray();
                        return View(ad);
                    }
                }
            }

            ad.Title = advertisement.Title;
            ad.Description = advertisement.Description;
            ad.UpdatedAt = DateTime.Now;

            // Update categories
            ad.Categories.Clear();
            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                foreach (var catId in selectedCategories)
                {
                    var category = _context.Categories.FirstOrDefault(c => c.Id == catId);
                    if (category != null)
                    {
                        ad.Categories.Add(category);
                    }
                }
            }

            // Delete selected media
            if (mediaToDelete != null && mediaToDelete.Length > 0)
            {
                foreach (var mediaId in mediaToDelete)
                {
                    var media = ad.Media.FirstOrDefault(m => m.Id == mediaId);
                    if (media != null)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        _context.Media.Remove(media);
                    }
                }
            }

            // Save new media files
            try
            {
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ads", ad.Id.ToString());
                Directory.CreateDirectory(uploadsRoot);

                if (mediaFiles != null && mediaFiles.Length > 0)
                {
                    foreach (var file in mediaFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName);
                            var savedName = Guid.NewGuid().ToString() + ext;
                            var savePath = Path.Combine(uploadsRoot, savedName);
                            using (var stream = new FileStream(savePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            var media = new AdvertisementMedia
                            {
                                AdvertisementId = ad.Id,
                                FileName = file.FileName,
                                FilePath = $"/uploads/ads/{ad.Id}/{savedName}",
                                MediaType = file.ContentType ?? "application/octet-stream",
                                UploadedAt = DateTime.Now
                            };
                            _context.Media.Add(media);
                        }
                    }
                }

                if (attachmentFiles != null && attachmentFiles.Length > 0)
                {
                    foreach (var file in attachmentFiles)
                    {
                        if (file != null && file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName);
                            var savedName = Guid.NewGuid().ToString() + ext;
                            var savePath = Path.Combine(uploadsRoot, savedName);
                            using (var stream = new FileStream(savePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            var attachment = new AdvertisementFile
                            {
                                AdvertisementId = ad.Id,
                                FileName = file.FileName,
                                FilePath = $"/uploads/ads/{ad.Id}/{savedName}",
                                FileSize = file.Length,
                                UploadedAt = DateTime.Now
                            };
                            _context.Files.Add(attachment);
                        }
                    }
                }
            }
            catch
            {
                // If saving files fails, continue
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = ad.Id });
        }

        // GET: /Advertisement/Delete/5
        public IActionResult Delete(int id)
        {
            var ad = _context.Advertisements
                .Include(a => a.User)
                .FirstOrDefault(a => a.Id == id);
                
            if (ad == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            
            if (userId == null || (ad.UserId != userId.Value && !isAdmin))
            {
                return Forbid();
            }

            return View(ad);
        }

        // POST: /Advertisement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var ad = _context.Advertisements
                .Include(a => a.Media)
                .Include(a => a.Files)
                .FirstOrDefault(a => a.Id == id);
                
            if (ad == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            
            if (userId == null || (ad.UserId != userId.Value && !isAdmin))
            {
                return Forbid();
            }

            // Delete media files
            foreach (var media in ad.Media)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Delete attachment files
            foreach (var file in ad.Files)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Advertisements.Remove(ad);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Advertisement/Search
        public IActionResult Search(string q = "", int page = 1, int pageSize = 10, int[]? categoryIds = null)
        {
            var query = q?.ToLower() ?? "";
            categoryIds = categoryIds ?? new int[0];

            var ads = _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Where(a => a.Status == AdvertisementStatus.Active &&
                    (string.IsNullOrEmpty(query) || 
                     a.Title.ToLower().Contains(query) || 
                     a.Description.ToLower().Contains(query)))
                .AsEnumerable(); // Switch to client evaluation for category filtering

            // Filter by categories if specified
            List<int> selectedCategoryIds = categoryIds.Where(id => id > 0).ToList();

            if (selectedCategoryIds.Any())
            {
                // Get all category IDs including subcategories
                var allCategoryIds = GetCategoryIdsWithChildren(selectedCategoryIds);
                
                ads = ads.Where(a => a.Categories.Any(c => allCategoryIds.Contains(c.Id)));
            }

            var adsList = ads.OrderByDescending(a => a.CreatedAt).ToList();

            int totalPages = (int)Math.Ceiling(adsList.Count / (double)pageSize);
            var pagedAds = adsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.TotalResults = adsList.Count;
            ViewBag.CategoryIds = string.Join(",", categoryIds);
            ViewBag.SelectedCategoryIds = selectedCategoryIds;
            
            // Get all categories for filter display
            ViewBag.AllCategories = _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToList();
            
            // Get selected categories
            if (selectedCategoryIds.Any())
            {
                var selectedCategories = _context.Categories.Where(c => selectedCategoryIds.Contains(c.Id)).ToList();
                ViewBag.SelectedCategories = selectedCategories;
            }

            return View(pagedAds);
        }

        // POST: /Advertisement/DeleteMedia/5
        [HttpPost]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var media = await _context.Media.FindAsync(id);
            if (media == null)
            {
                return NotFound();
            }

            // Check authorization
            var advertisement = await _context.Advertisements.FindAsync(media.AdvertisementId);
            if (advertisement == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            if (userId != advertisement.UserId && !isAdmin)
            {
                return Forbid();
            }

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete from database
            _context.Media.Remove(media);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: /Advertisement/DeleteFile/5
        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            // Check authorization
            var advertisement = await _context.Advertisements.FindAsync(file.AdvertisementId);
            if (advertisement == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            if (userId != advertisement.UserId && !isAdmin)
            {
                return Forbid();
            }

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Delete from database
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: /Advertisement/Report/5
        [HttpPost]
        public async Task<IActionResult> Report(int id, string reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Musisz być zalogowany, aby zgłosić ogłoszenie." });
            }

            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return Json(new { success = false, message = "Ogłoszenie nie istnieje." });
            }

            // Check if user is not reporting their own advertisement
            if (advertisement.UserId == userId)
            {
                return Json(new { success = false, message = "Nie możesz zgłosić własnego ogłoszenia." });
            }

            // Check if user already reported this advertisement
            var existingReport = await _context.ModerationReports
                .FirstOrDefaultAsync(r => r.AdvertisementId == id && r.ReportedByUserId == userId.Value && r.Status == ModerationReportStatus.Pending);
            
            if (existingReport != null)
            {
                return Json(new { success = false, message = "Już zgłosiłeś to ogłoszenie." });
            }

            var report = new ModerationReport
            {
                AdvertisementId = id,
                ReportedByUserId = userId.Value,
                Reason = reason ?? "Brak powodu",
                Status = ModerationReportStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.ModerationReports.Add(report);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ogłoszenie zostało zgłoszone do moderacji." });
        }

        // GET: /Advertisement/AdvancedSearch
        public IActionResult AdvancedSearch(int? categoryId)
        {
            ViewBag.Categories = _context.Categories.Where(c => c.ParentCategoryId == null).ToList();
            
            if (categoryId.HasValue)
            {
                var category = _context.Categories
                    .Include(c => c.Attributes)
                        .ThenInclude(a => a.Dictionary)
                            .ThenInclude(d => d.Values)
                    .FirstOrDefault(c => c.Id == categoryId.Value);
                
                ViewBag.SelectedCategory = category;
            }

            return View();
        }

        // POST: /Advertisement/AdvancedSearch
        [HttpPost]
        public async Task<IActionResult> AdvancedSearch(int? categoryId, Dictionary<string, string> criteria, int page = 1, int pageSize = 10)
        {
            var results = await _searchService.AdvancedSearchAsync(criteria, categoryId);

            int totalPages = (int)Math.Ceiling(results.Count / (double)pageSize);
            var pagedResults = results
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View("Search", pagedResults);
        }

        // GET: /Advertisement/DownloadFile
        [HttpGet]
        public IActionResult DownloadFile(int fileId)
        {
            var file = _context.Files.FirstOrDefault(f => f.Id == fileId);
            if (file == null)
            {
                return NotFound("Plik nie został znaleziony w bazie danych");
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Plik fizyczny nie istnieje: {filePath}");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", file.FileName);
        }

        // Helper method to get all category IDs including children recursively
        private List<int> GetCategoryIdsWithChildren(List<int> parentIds)
        {
            var result = new List<int>(parentIds);
            
            var categories = _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => parentIds.Contains(c.Id))
                .ToList();
            
            foreach (var category in categories)
            {
                if (category.ChildCategories != null && category.ChildCategories.Any())
                {
                    var childIds = category.ChildCategories.Select(c => c.Id).ToList();
                    result.AddRange(GetCategoryIdsWithChildren(childIds));
                }
            }
            
            return result.Distinct().ToList();
        }
    }
}
