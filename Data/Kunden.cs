using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebCodesBares.Data.Models
{
    public class Kunden
    {
        [Key] // 🔥 Définit la clé primaire
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Kunden { get; set; }
        
        public string? Firmenname { get; set; } = string.Empty;
        public string? NameKunden { get; set; } = string.Empty;
        public string? Anrede { get; set; } = string.Empty;
        public DateTime? Geburtsdatum { get; set; }
        public string? Kundentyp { get; set; } = string.Empty;
        public string? Adresse { get; set; } = string.Empty;
        public string? Telefonnummer { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Webseite { get; set; } = string.Empty;
        public string? SocialMediaProfile { get; set; } = string.Empty;
        public string? UmsatzstuerID { get; set; } = string.Empty;
        public string? Handelsregisternummer { get; set; } = string.Empty;
        public string? Branche { get; set; } = string.Empty;
        public int? Mitarbeiteranzahl { get; set; }
        public string? Firmensitz { get; set; } = string.Empty;
        public string? AnsprechpartnerName { get; set; } = string.Empty;
        public string? PositionImUnternehmen { get; set; } = string.Empty;
        public string? TelefoneAnsprech { get; set; } = string.Empty;
        public string? EmailAnsprech { get; set; } = string.Empty;
        public string? Kaufhistorie { get; set; } = string.Empty;
        public string? Vertragsdetails { get; set; } = string.Empty;
        public string? Abonnements { get; set; } = string.Empty;
        public string? Zahlungsbedingungen { get; set; } = string.Empty;
        public string? Zahlungsmethode { get; set; } = string.Empty;
        public string? Bankverbindung { get; set; } = string.Empty;
        public string? OffeneRechnungen { get; set; } = string.Empty;
        public string? Mahnstatus { get; set; } = string.Empty;
        public string? Kommunikationsprotokoll { get; set; } = string.Empty;
        public string? SupportTickets { get; set; } = string.Empty;
        public string? Feedback { get; set; } = string.Empty;
        public string? Notizen { get; set; } = string.Empty;
        [ForeignKey("Mitarbeiter")]
        public int? Id_Mitarbeiter { get; set; }
        public Mitarbeiter? Mitarbeiter { get; set; }
     
    }
}
