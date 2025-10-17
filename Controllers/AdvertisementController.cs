using Microsoft.AspNetCore.Mvc;
using Projekt_MVC.Models;
using System.Collections.Generic;
using System.Linq;

namespace Projekt_MVC.Controllers
{
    public class AdvertisementController : Controller
    {
        // Symulacja "bazy danych" w pamięci
        private static List<Advertisement> _advertisements = new List<Advertisement>();
        private static int _nextId = 1;

        public AdvertisementController()
        {
            // Dodajmy przykładowe dane jeśli lista jest pusta
            if (!_advertisements.Any())
            {
                _advertisements.AddRange(new List<Advertisement>
                {
                    new Advertisement { Id = _nextId++, Title = "Sprzedam Opla", Description = "Bardzo sprawny samochód", Price = 5000, UserId = 1, CreatedAt = System.DateTime.Now },
                    new Advertisement { Id = _nextId++, Title = "Kupię rower", Description = "Dowolny model, stan dobry", Price = 200, UserId = 2, CreatedAt = System.DateTime.Now }
                });
            }
        }

        // GET: /Advertisement
        public IActionResult Index()
        {
            return View(_advertisements);
        }

        // GET: /Advertisement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Advertisement/Create
        [HttpPost]
        public IActionResult Create(Advertisement advertisement)
        {
            // TYMCZASOWO POMIŃ WALIDACJĘ - zapisz wszystko
            advertisement.Id = _nextId++;
            advertisement.CreatedAt = DateTime.Now;
            _advertisements.Add(advertisement);
            return RedirectToAction(nameof(Index));
        }
    }
}