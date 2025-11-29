using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_MVC.Data;
using Projekt_MVC.Models;

namespace Projekt_MVC.Controllers
{
    public class AdvertisementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdvertisementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Advertisement
        public async Task<IActionResult> Index()
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.User)
                .Include(a => a.Categories)
                .ToListAsync();
            return View(advertisements);
        }

        // GET: /Advertisement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Advertisement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Advertisement advertisement)
        {
            if (ModelState.IsValid)
            {
                advertisement.CreatedAt = DateTime.Now;
                advertisement.UserId = 1;
                
                _context.Advertisements.Add(advertisement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(advertisement);
        }
    }
}
