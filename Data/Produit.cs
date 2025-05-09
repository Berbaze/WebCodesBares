using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebCodesBares.Data
{
    public class Produit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Produit { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Prix { get; set; } 

        public int Quantite { get; set; } = 1;

        public bool EstLicenceVolume { get; set; } = false; 

        public int Stock { get; set; } = 0;

        public string? ImageUrl { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
        public string LienTelechargement { get; set; } = string.Empty;
        public virtual ICollection<CommandeProduit> CommandeProduits { get; set; } = new List<CommandeProduit>();
    }
}
