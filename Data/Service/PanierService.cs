using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using WebCodesBares.Helpers;
using WebCodesBares.Services; 
// ✅ Extension pour gérer la session

namespace WebCodesBares.Data.Service
{
    public class PanierService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "Panier";

        public PanierService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 🔹 Récupérer la liste des produits dans le panier
        public List<int> GetPanier()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.GetObject<List<int>>(SessionKey) ?? new List<int>();
        }

        // 🔹 Ajouter un produit au panier
        public void AjouterProduit(int produitId)
        {
            var panier = GetPanier();
            panier.Add(produitId);
            _httpContextAccessor.HttpContext?.Session.SetObject(SessionKey, panier);
        }

        // 🔹 Supprimer un produit du panier
        public void SupprimerProduit(int produitId)
        {
            var panier = GetPanier();
            panier.Remove(produitId);
            _httpContextAccessor.HttpContext?.Session.SetObject(SessionKey, panier);
        }

        // 🔹 Obtenir les produits complets du panier (avec les détails)
        public List<Produit> ObtenirProduitsDuPanier()
        {
            var panierIds = GetPanier();
            return _context.Produit.Where(p => panierIds.Contains(p.Id_Produit)).ToList();
        }

        // 🔹 Obtenir le nombre total de produits dans le panier
        public int GetNombreProduits()
        {
            return GetPanier().Count;
        }

        // 🔹 Vider le panier
        public void ViderPanier()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

    }
}
