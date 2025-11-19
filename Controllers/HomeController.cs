using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ogloszenia.Models;
using ogloszenia.Services;

namespace ogloszenia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Get latest 6 advertisements
            var latestAds = InMemoryDatabase.Advertisements
                .Where(a => a.Status == AdvertisementStatus.Active)
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToList();

            // Get admin message
            var adminMessage = InMemoryDatabase.SystemSettings.AdminMessage;

            ViewBag.LatestAds = latestAds;
            ViewBag.AdminMessage = adminMessage;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
