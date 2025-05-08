using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Orders;
using System.Text.Json;
using System.Threading.Tasks;
using WebCodesBares.Services;
using WebCodesBares.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using WebCodesBares.Data.Models;

namespace WebCodesBares.Controllers
{
    [Route("api/paypal")]
    [ApiController]
    public class PayPalController : ControllerBase
    {
        private readonly PayPalService _paypalService;
        private readonly ILogger<PayPalController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public PayPalController(PayPalService paypalService,
                                ILogger<PayPalController> logger,
                                ApplicationDbContext dbContext,
                                UserManager<ApplicationUser> userManager)
        {
            _paypalService = paypalService;
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
        }
        [HttpGet("/api/utilisateur/estConnecte")]
        public IActionResult EstUtilisateurConnecte()
        {
            if (User.Identity.IsAuthenticated)
            {
                return new JsonResult(new { estConnecte = true });
            }
            return new JsonResult(new { estConnecte = false });
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] JsonElement data)
        {
            try
            {
                _logger.LogInformation("📥 Données reçues : {Data}", data.ToString());

                // Extraction du montant
                if (!data.TryGetProperty("montant", out JsonElement montantElement) || montantElement.ValueKind != JsonValueKind.Number)
                {
                    _logger.LogWarning("⚠️ Montant invalide ou manquant dans les données.");
                    return BadRequest("Montant invalide ou manquant.");
                }
                decimal montant = montantElement.GetDecimal();
                if (montant <= 0)
                {
                    _logger.LogWarning("⚠️ Le montant doit être supérieur à zéro.");
                    return BadRequest("Le montant doit être supérieur à zéro.");
                }

                // Extraction de la liste des produits (attendu dans le JSON)
                List<Produit> produits = new List<Produit>();
                if (data.TryGetProperty("produits", out JsonElement produitsElement) && produitsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in produitsElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("Id_Produit", out JsonElement idProduitElement) && idProduitElement.ValueKind == JsonValueKind.Number)
                        {
                            int idProduit = idProduitElement.GetInt32();
                            var produit = await _dbContext.Produit.FirstOrDefaultAsync(p => p.Id_Produit == idProduit);
                            if (produit != null)
                            {
                                produits.Add(produit);
                            }
                        }
                    }
                }
                if (!produits.Any())
                {
                    _logger.LogWarning("⚠️ Aucun produit valide trouvé dans la requête.");
                    return BadRequest("Aucun produit valide trouvé.");
                }

                // Récupérer l'utilisateur connecté
                var client = await _userManager.GetUserAsync(User);
                if (client == null)
                {
                    _logger.LogWarning("⚠️ Utilisateur non connecté ou introuvable !");
                    return Unauthorized("Utilisateur non connecté ou introuvable.");
                }

                // Définition des URL de retour et d'annulation
                string returnUrl = $"{Request.Scheme}://{Request.Host}/api/paypal/success";
                string cancelUrl = $"{Request.Scheme}://{Request.Host}/api/paypal/cancel";

                // Création de la commande locale via PayPalService (enregistre aussi les produits)
                var commande = await _paypalService.CreateOrder(montant, returnUrl, cancelUrl, client, produits);
                if (commande == null)
                {
                    _logger.LogError("🚨 Échec de la création de la commande PayPal.");
                    return BadRequest("Échec de la création de la commande PayPal.");
                }

                _logger.LogInformation("✅ Commande PayPal créée avec PayPalId : {PayPalId}", commande.PayPalId);
                return Ok(new { orderId = commande.PayPalId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la création de la commande PayPal.");
                return BadRequest($"Erreur lors de la création de la commande PayPal : {ex.Message}");
            }
        }

        [HttpPost("capture-order/{orderId}")]
        public async Task<IActionResult> CaptureOrder(string orderId)
        {
            try
            {
                _logger.LogInformation("🔄 Vérification et capture du paiement pour Order ID PayPal : {OrderId}", orderId);

                // 🧑‍💻 Récupération utilisateur connecté
                var client = await _userManager.GetUserAsync(User);
                if (client == null)
                {
                    _logger.LogWarning("⚠️ Utilisateur non trouvé !");
                    return Unauthorized(new { message = "Utilisateur non connecté." });
                }

                // 🛒 Produits de la commande
                var commande = await _dbContext.Commande
                    .Include(c => c.CommandeProduits)
                        .ThenInclude(cp => cp.Produit)
                    .FirstOrDefaultAsync(c => c.PayPalId == orderId);

                if (commande == null || commande.CommandeProduits == null || !commande.CommandeProduits.Any())
                {
                    _logger.LogWarning("⚠️ Aucun produit trouvé pour la commande PayPal : {OrderId}", orderId);
                    return BadRequest(new { message = "Aucun produit associé à cette commande." });
                }

                var produits = commande.CommandeProduits.Select(cp => cp.Produit).ToList();

                // ✅ Appel de la vraie méthode qui gère tout (paiement, licence, synology, email)
                var order = await _paypalService.CaptureOrder(orderId, client, produits);
                if (order == null)
                {
                    return BadRequest(new { message = "Capture échouée." });
                }

                return Ok(new
                {
                    message = "✅ Paiement capturé + traitement licence effectué.",
                    commandeId = commande.Id_Commande
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la capture complète.");
                return StatusCode(500, new { message = "Erreur interne serveur." });
            }
        }



        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery] string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogWarning("⚠️ Aucun ID de commande PayPal fourni !");
                return RedirectToPage("/Erreur");
            }

            var commande = await _dbContext.Commande.FirstOrDefaultAsync(c => c.PayPalId == orderId);
            if (commande == null)
            {
                _logger.LogWarning("⚠️ Commande non trouvée en base pour Order ID PayPal : {orderId}", orderId);
                return RedirectToPage("/Erreur");
            }

            _logger.LogInformation("✅ Paiement réussi pour Order ID PayPal : {orderId}, redirection vers Confirmation avec Commande ID : {commandeId}", orderId, commande.Id_Commande);
            return RedirectToPage("/Comande/Confirmation", new { orderId = commande.Id_Commande });
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            _logger.LogInformation("❌ Paiement annulé par l'utilisateur.");
            return RedirectToPage("/EinLogen/Login");
        }
       
    }
}
