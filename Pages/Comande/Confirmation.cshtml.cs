using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Service;
using WebCodesBares.Data.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

public class ConfirmationModel : PageModel
{
    private readonly ILogger<ConfirmationModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly LicenceService _licenceService;
    private readonly IEmailSender _emailSender; // Injection via interface

    [BindProperty(SupportsGet = true)]
    public string PayPalId { get; set; }

    public Commande Commande { get; set; }

    public ConfirmationModel(
        ILogger<ConfirmationModel> logger,
        ApplicationDbContext context,
        LicenceService licenceService,
        IEmailSender emailSender) // Injection Interface
    {
        _logger = logger;
        _context = context;
        _licenceService = licenceService;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("🔍 Überprüfung der empfangenen PayPal-ID: {PayPalId}", PayPalId);

        if (string.IsNullOrWhiteSpace(PayPalId))
        {
            _logger.LogWarning("⚠️ Die PayPal-ID ist leer oder ungültig!");
            return RedirectToPage("/Erreur");
        }

        Commande = await _context.Commande
            .Include(c => c.CommandeProduits)
            .ThenInclude(cp => cp.Produit)
            .FirstOrDefaultAsync(c => c.PayPalId == PayPalId);

        if (Commande == null)
        {
            _logger.LogWarning("⚠️ Keine Bestellung gefunden für die PayPal-ID: {PayPalId}", PayPalId);
            return RedirectToPage("/Erreur");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Commande.ClientId);
        if (user == null)
        {
            _logger.LogError("❌ Benutzer nicht gefunden für die Bestellung {CommandeId}", Commande.Id_Commande);
            return RedirectToPage("/Erreur");
        }

        _logger.LogInformation("👤 Benutzer gefunden : {UserName} ({Email})", user.UserName, user.Email);

        var emailBody = new StringBuilder();
        emailBody.AppendLine($"Hallo {user.UserName},");
        emailBody.AppendLine("Vielen Dank für Ihren Kauf! Hier ist Ihre Lizenz:");

        foreach (var commandeProduit in Commande.CommandeProduits)
        {
            _logger.LogInformation("🛠 Lizenz wird erstellt für Produkt : {ProduitNom}", commandeProduit.Produit.Nom);

            try
            {
                var licence = await _licenceService.CreerLicenceAsync(Commande, commandeProduit.Produit, user);

                if (licence != null)
                {
                    _logger.LogInformation("✅ Lizenz gespeichert: {Cle}", licence.Cle);
                    emailBody.AppendLine($"\n🔑 Lizenz für {commandeProduit.Produit.Nom} : {licence.Cle}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lizenzerstellung fehlgeschlagen für Produkt {ProduitNom}", commandeProduit.Produit.Nom);
            }
        }

        emailBody.AppendLine("\nMit freundlichen Grüßen, \nIhr ArchivCode-Team");

        string subject = "🎉 Ihre ArchivCode-Lizenz ist bereit!";

        try
        {
            await _emailSender.SendEmailAsync(user.Email, subject, emailBody.ToString());
            _logger.LogInformation("📩 Bestätigungs-E-Mail gesendet an {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fehler beim Senden der E-Mail an {Email}", user.Email);
        }

        return Page();
    }

}
