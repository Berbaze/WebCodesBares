using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using Microsoft.Extensions.Logging;

namespace WebCodesBares.Pages.Produits
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public Produit Produit { get; set; } = new Produit();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<CreateModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public void OnGet()
        {
            // Affichage initial
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("🚀 OnPostAsync() a été appelée !");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ ModelState est invalide !");
                return Page();
            }

            // S'assurer que l'ID est généré par SQL Server
            Produit.Id_Produit = 0;

            // Gestion de l'upload d'image
            if (ImageFile != null)
            {
                _logger.LogInformation("📸 Une image a été sélectionnée.");
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                Produit.ImageUrl = "/images/" + uniqueFileName;
            }

            _context.Produit.Add(Produit);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Produit '{Produit.Nom}' ajouté avec succès ! (ID: {Produit.Id_Produit})");

            TempData["SuccessMessage"] = $"Le produit '{Produit.Nom}' a été ajouté avec succès !";

            return RedirectToPage("./Index");
        }
    }
}
