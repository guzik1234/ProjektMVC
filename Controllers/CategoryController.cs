using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Data;
using Microsoft.EntityFrameworkCore;

namespace ogloszenia.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Category/Index
        public async Task<IActionResult> Index()
        {
            var rootCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .ToListAsync();
            return View(rootCategories);
        }

        // GET: /Category/Details/5
        public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 10)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            var ads = await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Where(a => a.Status == AdvertisementStatus.Active && a.Categories.Any(c => c.Id == id))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

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
