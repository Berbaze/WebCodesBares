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

      

    

        return Page();
    }

}
