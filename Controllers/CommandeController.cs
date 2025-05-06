using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Service;
using WebCodesBares.Helpers;
using WebCodesBares.Services;

namespace WebCodesBares.Controllers
{
    [Route("Commande")]
    public class CommandeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly ILogger<CommandeController> _logger;
        private readonly PayPalService _payPalService;

        public CommandeController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager,
                                  EmailService emailService,
                                  PayPalService payPalService,
                                  ILogger<CommandeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _payPalService = payPalService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpGet("valider-achat-paypal")]
        public async Task<IActionResult> ValiderAchatPaypal(string paypalOrderId)
        {
            _logger.LogInformation("🚀 Début de la validation de l'achat via PayPal...");

            if (string.IsNullOrEmpty(paypalOrderId))
            {
                _logger.LogError("❌ L'ID de commande PayPal est nul ou vide.");
                return BadRequest("L'ID de la commande PayPal est requis.");
            }

            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogWarning("🔒 Utilisateur non authentifié.");
                return Unauthorized("Veuillez vous connecter pour valider l'achat.");
            }

            var client = await _userManager.FindByIdAsync(clientId);
            if (client == null)
            {
                _logger.LogWarning("❌ Utilisateur introuvable.");
                return BadRequest("Utilisateur introuvable.");
            }

            var panierIds = HttpContext.Session.GetObjectFromJson<List<int>>("Panier") ?? new List<int>();
            if (!panierIds.Any())
            {
                _logger.LogWarning("⚠️ Le panier est vide.");
                return BadRequest("Votre panier est vide.");
            }

            var produits = await _context.Produit
                                         .Where(p => panierIds.Contains(p.Id_Produit))
                                         .ToListAsync();
            if (!produits.Any())
            {
                _logger.LogWarning("📦 Aucun produit valide trouvé pour les IDs du panier.");
                return BadRequest("Les produits du panier sont invalides.");
            }

            try
            {
                // ✅ Capture de la commande PayPal avec les produits
                var order = await _payPalService.CaptureOrder(paypalOrderId, client, produits);
                if (order == null || order.Status != "COMPLETED")
                {
                    _logger.LogError("❌ Échec de la capture du paiement PayPal.");
                    return BadRequest("La transaction PayPal n'a pas pu être complétée.");
                }

                _logger.LogInformation("🎉 Transaction validée avec succès !");
                HttpContext.Session.SetObjectAsJson("Panier", new List<int>());

                return RedirectToAction("Confirmation", "Comande", new { payPalId = paypalOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Erreur lors de la validation de l'achat.");
                return BadRequest($"Erreur : {ex.Message}");
            }
        }
    }
}
