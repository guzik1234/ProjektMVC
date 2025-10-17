using Microsoft.AspNetCore.Mvc;
using Projekt_MVC.Models;  // ← WAŻNE: użyj prawidłowego namespace
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Projekt_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static List<Advertisement> _advertisements = new List<Advertisement>();
        private static int _nextId = 1;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            if (!_advertisements.Any())
            {
                _advertisements.AddRange(new List<Advertisement>
                {
                    new Advertisement { Id = _nextId++, Title = "Sprzedam Opla", Description = "Bardzo sprawny samochód", Price = 5000, UserId = 1 },
                    new Advertisement { Id = _nextId++, Title = "Kupię rower", Description = "Dowolny model, stan dobry", Price = 200, UserId = 2 },
                });
            }
        }

        public IActionResult Index()
        {
            return View(_advertisements);
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