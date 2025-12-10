using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ogloszenia.Models;
using ogloszenia.Data;
using ogloszenia.Services;

namespace ogloszenia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IRssService _rssService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IRssService rssService)
        {
            _logger = logger;
            _context = context;
            _rssService = rssService;
        }

        public IActionResult Index()
        {
            // Get latest 6 advertisements
            var latestAds = _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Include(a => a.Media)
                .Where(a => a.Status == AdvertisementStatus.Active)
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToList();

            ViewBag.LatestAds = latestAds;

            // Get admin message
            var settings = _context.SystemSettings.FirstOrDefault();
            ViewBag.AdminMessage = settings?.AdminMessage ?? "";

            // Get all main categories for search filter
            ViewBag.AllCategories = _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Rss()
        {
            var rssContent = await _rssService.GenerateRssFeedAsync();
            return Content(rssContent, "application/rss+xml; charset=utf-8");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
