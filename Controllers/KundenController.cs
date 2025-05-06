using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;

namespace WebCodesBares.Controllers
{
    public class KundenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KundenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Affiche le formulaire d'ajout de client
        public IActionResult Create()
        {
            return View();
        }

        // Enregistre un client en base de données
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kunden kunde)
        {
            if (ModelState.IsValid)
            {
                _context.Kunden.Add(kunde);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index"); // Redirection après l'ajout
            }
            return View(kunde);
        }

        // Affiche la liste des clients
        public async Task<IActionResult> Index()
        {
            var kundenList = await _context.Kunden.ToListAsync();
            return View(kundenList);
        }

    }
}
