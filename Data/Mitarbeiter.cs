using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCodesBares.Data.Models
{
    public class Mitarbeiter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; } // L'ID de l'utilisateur employé

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string? AdminId { get; set; } // L'ID de l'administrateur  

        [ForeignKey("AdminId")]
        public ApplicationUser? Admin { get; set; }

        [Required]
        public int LicenceId { get; set; } // Lien vers la licence de l'admin

        [ForeignKey("LicenceId")]
        public Licence? Licence { get; set; }
    }
}
