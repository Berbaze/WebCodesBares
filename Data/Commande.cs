using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebCodesBares.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace WebCodesBares.Data
{
    public class Commande
    {
        [Key]
        public int Id_Commande { get; set; }

        // Utiliser 'Id' de type string (ASP.NET Identity)
        public string? ClientId { get; set; }

        public DateTime DateAchat { get; set; }
        public bool EstPaye { get; set; }

        // Relation directe avec 'IdentityUser'
        [ForeignKey("ClientId")]
        public ApplicationUser? Client { get; set; }
        public string? PayPalId { get; set; } = string.Empty;
        // 🔹 Relation avec les produits
        public virtual ICollection<CommandeProduit> CommandeProduits { get; set; } = new List<CommandeProduit>();
    }
}
