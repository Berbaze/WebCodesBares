using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebCodesBares.Data
{
    public class Produit
    {
        [Key] // 🔥 Définit la clé primaire
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Produit { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")] // ✅ Correction : précision SQL pour éviter les troncatures
        public decimal Prix { get; set; }  // ✅ Suppression du `?` (ne doit pas être nullable)

        public int Quantite { get; set; } = 1;

        public bool EstLicenceVolume { get; set; } = false; // ✅ Suppression du `?` (booléen doit être défini)

        public int Stock { get; set; } = 0;

        public string? ImageUrl { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        // 🔹 Nouveau champ pour le lien de téléchargement
        public string LienTelechargement { get; set; } = string.Empty;

        // 🔹 Relation avec CommandeProduit (relation N-N avec Commande)
        public virtual ICollection<CommandeProduit> CommandeProduits { get; set; } = new List<CommandeProduit>();
    }
}
