using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Services;

namespace ogloszenia.Controllers
{
    public class CategoryController : Controller
    {
        private readonly List<Category> _categories;
        private readonly List<Advertisement> _advertisements;

        public CategoryController()
        {
            _categories = InMemoryDatabase.Categories;
            _advertisements = InMemoryDatabase.Advertisements;
        }

        // GET: /Category/Index
        public IActionResult Index()
        {
            var rootCategories = _categories.Where(c => c.ParentCategoryId == null).ToList();
            return View(rootCategories);
        }

        // GET: /Category/Details/5
        public IActionResult Details(int id, int page = 1, int pageSize = 10)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            var ads = _advertisements
                .Where(a => a.Status == AdvertisementStatus.Active && a.Categories.Any(c => c.Id == id))
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            int totalPages = (int)Math.Ceiling(ads.Count / (double)pageSize);
            var pagedAds = ads
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Category = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(pagedAds);
        }
    }
}
