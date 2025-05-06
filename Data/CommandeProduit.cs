using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCodesBares.Data
{
    public class CommandeProduit
    {
        [Key] // 🔥 Définit la clé primaire
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Clé primaire

        [ForeignKey("Commande")]
        public int Id_Commande { get; set; }
        public Commande? Commande { get; set; }

        [ForeignKey("Produit")]
        public int Id_Produit { get; set; }
        public virtual Produit Produit { get; set; }

    }
}
