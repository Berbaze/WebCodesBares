using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using WebCodesBares.Helpers; // Pour l'extension de session
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WebCodesBares.Data.Service;
using Microsoft.EntityFrameworkCore;

namespace WebCodesBares.Pages.Produits
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly PanierService _panierService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext context, PanierService panierService, ILogger<IndexModel> logger)
        {
            _context = context;
            _panierService = panierService;
            _logger = logger;
        }

        // Propriétés pour les licences (Basic, Pro, Enterprise)
        public List<Produit> BasicListe { get; set; } = new();
        public List<Produit> ProListe { get; set; } = new();
        public List<Produit> EnterpriseListe { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Filtrer les produits en fonction de leur type de licence
                BasicListe = await _context.Produit
        .Where(p => EF.Functions.Like(p.Type.ToLower(), "%basic%"))
        .ToListAsync();

                ProListe = await _context.Produit
                    .Where(p => EF.Functions.Like(p.Type.ToLower(), "%pro%"))
                    .ToListAsync();

                EnterpriseListe = await _context.Produit
                    .Where(p => EF.Functions.Like(p.Type.ToLower(), "%enterprise%"))
                    .ToListAsync();


                _logger.LogInformation("Produkte geladen: Basic={0}, Pro={1}, Enterprise={2}",
                    BasicListe.Count, ProListe.Count, EnterpriseListe.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors du chargement des produits : {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAjouterAuPanierAsync(int produitId)
        {
            var produit = await _context.Produit.FindAsync(produitId);
            if (produit == null)
            {
                _logger.LogWarning($"Produit avec ID {produitId} introuvable.");
                return NotFound();
            }

            _panierService.AjouterProduit(produitId);
            _logger.LogInformation($"Produit ID {produitId} ajouté au panier.");
            return RedirectToPage();
        }
    }
}
