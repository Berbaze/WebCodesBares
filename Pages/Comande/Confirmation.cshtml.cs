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
        _logger.LogInformation("🔍 Vérification de l'ID PayPal reçu : {PayPalId}", PayPalId);

        if (string.IsNullOrWhiteSpace(PayPalId))
        {
            _logger.LogWarning("⚠️ L'ID PayPal est vide ou invalide !");
            return RedirectToPage("/Erreur");
        }

        Commande = await _context.Commande
            .Include(c => c.CommandeProduits)
            .ThenInclude(cp => cp.Produit)
            .FirstOrDefaultAsync(c => c.PayPalId == PayPalId);

        if (Commande == null)
        {
            _logger.LogWarning("⚠️ Aucune commande trouvée pour l'ID PayPal : {PayPalId}", PayPalId);
            return RedirectToPage("/Erreur");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Commande.ClientId);
        if (user == null)
        {
            _logger.LogError("❌ Utilisateur introuvable pour la commande {CommandeId}", Commande.Id_Commande);
            return RedirectToPage("/Erreur");
        }

        _logger.LogInformation("👤 Utilisateur trouvé : {UserName} ({Email})", user.UserName, user.Email);

        var emailBody = new StringBuilder();
        emailBody.AppendLine($"Bonjour {user.UserName},");
        emailBody.AppendLine("Merci pour votre achat ! Voici votre/vos licence(s) :");

        foreach (var commandeProduit in Commande.CommandeProduits)
        {
            _logger.LogInformation("🛠 Création d'une licence pour le produit : {ProduitNom}", commandeProduit.Produit.Nom);

            try
            {
                var licence = await _licenceService.CreerLicenceAsync(Commande, commandeProduit.Produit, user);

                if (licence != null)
                {
                    _logger.LogInformation("✅ Licence enregistrée : {Cle}", licence.Cle);
                    emailBody.AppendLine($"\n🔑 Licence pour {commandeProduit.Produit.Nom} : {licence.Cle}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Échec de la création de licence pour le produit {ProduitNom}", commandeProduit.Produit.Nom);
            }
        }

        emailBody.AppendLine("\nCordialement, \nL'équipe WebCodesBares");

        string subject = "🎉 Votre licence WebCodesBares est prête !";

        try
        {
            await _emailSender.SendEmailAsync(user.Email, subject, emailBody.ToString());
            _logger.LogInformation("📩 E-mail de confirmation envoyé à {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Échec de l'envoi de l'e-mail à {Email}", user.Email);
        }

        return Page();
    }

}
