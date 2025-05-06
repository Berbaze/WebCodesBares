using System.Collections.Generic;

namespace WebCodesBares.Data
{
    public class Panier
    {
        public List<Produit> Produits { get; set; } = new(); // ✅ Contient des objets Produit

        public decimal Total => Produits.Sum(p => p.Prix * p.Quantite);
        // ✅ Ajout du calcul du total
    }
}