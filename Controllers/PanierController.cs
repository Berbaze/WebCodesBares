using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using WebCodesBares.Data.Service;

namespace WebCodesBares.Controllers
{
    [Route("api/panier")]
    [ApiController]
    public class PanierController : ControllerBase
    {
        private readonly PanierService _panierService;
        private readonly ApplicationDbContext _dbContext;

        public PanierController(PanierService panierService, ApplicationDbContext dbContext)
        {
            _panierService = panierService;
            _dbContext = dbContext;
        }

        // GET: api/panier/details
        [HttpGet("details")]
        public async Task<IActionResult> GetPanierDetails()
        {
            // Récupérer la liste des IDs produits depuis le service de panier (par exemple, stockés en session)
            List<int> produitIds = _panierService.GetPanier();
            // Interroger la base de données pour obtenir les produits correspondants
            List<Produit> produits = await _dbContext.Produit
                .Where(p => produitIds.Contains(p.Id_Produit))
                .ToListAsync();
            // Calculer le total du panier
            decimal total = produits.Sum(p => p.Prix);
            return Ok(new { produits, totalPanier = total });
        }

        // GET: api/panier/count
        [HttpGet("count")]
        public IActionResult GetNombreProduits()
        {
            int count = _panierService.GetNombreProduits();
            return Ok(new { count });
        }

        // POST: api/panier/ajouter?produitId=123
        [HttpPost("ajouter")]
        public IActionResult AjouterProduit([FromQuery] int produitId)
        {
            if (produitId <= 0)
                return BadRequest(new { message = "ID produit invalide" });

            _panierService.AjouterProduit(produitId);
            int totalProduits = _panierService.GetNombreProduits();
            return Ok(new { count = totalProduits, message = "Produit ajouté avec succès !" });
        }

        // DELETE: api/panier/supprimer?produitId=123
        [HttpDelete("supprimer")]
        public IActionResult SupprimerProduit([FromQuery] int produitId)
        {
            _panierService.SupprimerProduit(produitId);
            int newCount = _panierService.GetNombreProduits();
            return Ok(new { message = "Produit supprimé du panier !", totalPanier = newCount });
        }

        // GET: api/panier/produits
        [HttpGet("produits")]
        public async Task<IActionResult> ObtenirProduitsDuPanier()
        {
            List<int> produitIds = _panierService.GetPanier();
            List<Produit> produits = await _dbContext.Produit
                .Where(p => produitIds.Contains(p.Id_Produit))
                .ToListAsync();
            return Ok(produits);
        }

        // POST: api/panier/vider
        [HttpPost("vider")]
        public IActionResult ViderPanier()
        {
            _panierService.ViderPanier();
            return Ok(new { message = "Le panier a été vidé." });
        }
    }
}
