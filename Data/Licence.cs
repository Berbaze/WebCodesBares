using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCodesBares.Data.Models
{
    public class Licence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Licence { get; set; }

        [Required]
        [StringLength(100)]
        public string Cle { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Par exemple, "Basic", "Pro", "Enterprise"

        [Required]
        public int NombreUtilisateurs { get; set; }

        [Required]
        public int NombreBarcodes { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Prix { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrixMaintenance { get; set; }

        [Required]
        public DateTime DateEmission { get; set; } = DateTime.Now;

        [Required]
        public DateTime DateExpiration { get; set; }

        [Required]
        public bool Active { get; set; } = true;

        [Required]
        public int Id_Commande { get; set; }

        [Required]
        public int BarcodesRestants { get; set; }

        [ForeignKey("Id_Commande")]
        public virtual Commande? Commande { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? Utilisateur { get; set; }

        public bool EstSuspendue { get; set; } = false;
        public DateTime? DatePause { get; set; }

    }
}
