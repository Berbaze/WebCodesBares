namespace WebCodesBares.Data
{
    public class KundenViewModel
    {
  
            public int Id_Kunden { get; set; }
            public string Firmenname { get; set; } = "Non renseigné";
            public string Email { get; set; } = "Aucune email";
            public string Telefonnummer { get; set; } = "Non disponible";
            public string Adresse { get; set; } = "Adresse inconnue";
            public string Webseite { get; set; } = "-";
            public string SocialMediaProfile { get; set; } = "-";
            public string UmsatzstuerID { get; set; } = "-";
            public string Handelsregisternummer { get; set; } = "-";
            public string Branche { get; set; } = "-";
            public int? Mitarbeiteranzahl { get; set; } = 0;
            public string Firmensitz { get; set; } = "-";
            public string AnsprechpartnerName { get; set; } = "Aucun contact";
            public string PositionImUnternehmen { get; set; } = "-";
            public string EmailAnsprech { get; set; } = "Aucune email";
            public string TelefoneAnsprech { get; set; } = "Non disponible";
            public string Kaufhistorie { get; set; } = "-";
            public string Vertragsdetails { get; set; } = "-";
            public string Abonnements { get; set; } = "-";
            public string Zahlungsbedingungen { get; set; } = "-";
            public string Zahlungsmethode { get; set; } = "-";
            public string Bankverbindung { get; set; } = "-";
            public string OffeneRechnungen { get; set; } = "-";
            public string Mahnstatus { get; set; } = "-";
            public string Kommunikationsprotokoll { get; set; } = "-";
            public string SupportTickets { get; set; } = "-";
            public string Feedback { get; set; } = "-";
            public string Notizen { get; set; } = "-";

            // ✅ Nouvelle propriété pour afficher le nom du Mitarbeiter
            public string MitarbeiterName { get; set; } = "Non attribué";
        }
    }


