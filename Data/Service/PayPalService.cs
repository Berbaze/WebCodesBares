using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using WebCodesBares.Data.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using PayPalPurchaseUnitRequest = PayPalCheckoutSdk.Orders.PurchaseUnitRequest;

namespace WebCodesBares.Services
{
    public class PayPalService
    {
        private readonly ILogger<PayPalService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PayPalHttpClient _paypalClient;
        private readonly IEmailSender _emailSender; // Changement ici
        private readonly ILoggerFactory _loggerFactory;
        private readonly LicenceService _licenceService;

        private readonly SynologyAuthService _synologyAuth;
        private readonly SynologyShareService _synologyShare;

        public PayPalService(
            ILogger<PayPalService> logger,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            ILoggerFactory loggerFactory,
            LicenceService licenceService,
            SynologyAuthService synologyAuth,
            SynologyShareService synologyShare)
        {
            _logger = logger;
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _loggerFactory = loggerFactory;
            _licenceService = licenceService;
            _synologyAuth = synologyAuth;
            _synologyShare = synologyShare;

            var clientId = configuration["PayPal:ClientId"];
            var clientSecret = configuration["PayPal:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("❌ Les identifiants PayPal ne sont pas configurés correctement.");
                throw new InvalidOperationException("Les identifiants PayPal sont manquants.");
            }

            var environment = new SandboxEnvironment(clientId, clientSecret);
            _paypalClient = new PayPalHttpClient(environment);
        }


        public async Task<Commande?> CreateOrder(decimal montant, string returnUrl, string cancelUrl, ApplicationUser client, List<Produit> produits)
        {
            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PayPalPurchaseUnitRequest>
                {
                    new PayPalPurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "EUR",
                            Value = montant.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                }
            });

            var response = await _paypalClient.Execute(request);
            var result = response.Result<Order>();

            if (result == null || string.IsNullOrEmpty(result.Id))
            {
                _logger.LogError("❌ Échec de la récupération de l'ID PayPal.");
                return null;
            }

            _logger.LogInformation("✅ Commande PayPal créée avec ID : {PayPalId}", result.Id);

            var commande = new Commande
            {
                ClientId = client.Id,
                DateAchat = DateTime.Now,
                EstPaye = false,
                PayPalId = result.Id
            };

            _dbContext.Commande.Add(commande);
            await _dbContext.SaveChangesAsync();

            var commandeProduits = produits.Select(p => new CommandeProduit
            {
                Id_Commande = commande.Id_Commande,
                Id_Produit = p.Id_Produit
            }).ToList();

            await _dbContext.CommandeProduit.AddRangeAsync(commandeProduits);
            await _dbContext.SaveChangesAsync();

            return commande;
        }

        public async Task<Order?> CaptureOrder(string orderId, ApplicationUser user, List<Produit> produits)
        {
            _logger.LogInformation("🔄 Vérification du statut de la commande PayPal : {OrderId}", orderId);

            if (produits == null || !produits.Any())
            {
                _logger.LogWarning("🚫 Liste de produits vide.");
                return null;
            }

            string paymentStatus = await GetPaymentStatusAsync(orderId);
            _logger.LogInformation("🔁 Statut du paiement reçu : {Status}", paymentStatus);

            if (paymentStatus != "APPROVED")
            {
                _logger.LogWarning("⚠️ Paiement non approuvé pour : {OrderId}", orderId);
                return null;
            }

            if (!await CapturePaymentAsync(orderId))
            {
                _logger.LogWarning("⚠️ Capture paiement échouée : {OrderId}", orderId);
                return null;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var commande = await _dbContext.Commande
                    .Include(c => c.CommandeProduits)
                    .FirstOrDefaultAsync(c => c.PayPalId == orderId);

                if (commande == null)
                {
                    _logger.LogError("❌ Commande introuvable pour OrderId : {OrderId}", orderId);
                    return null;
                }

                if (!commande.EstPaye)
                {
                    commande.EstPaye = true;
                    _dbContext.Commande.Update(commande);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("💾 Statut commande mis à jour.");
                }

                var existingProductIds = commande.CommandeProduits.Select(cp => cp.Id_Produit).ToHashSet();
                var newCommandeProduits = produits
                    .Where(p => !existingProductIds.Contains(p.Id_Produit))
                    .Select(p => new CommandeProduit
                    {
                        Id_Commande = commande.Id_Commande,
                        Id_Produit = p.Id_Produit
                    }).ToList();

                if (newCommandeProduits.Any())
                {
                    await _dbContext.CommandeProduit.AddRangeAsync(newCommandeProduits);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("➕ Produits ajoutés à la commande.");
                }

                var licences = new List<Licence>();

                foreach (var produit in produits)
                {
                    var produitDb = await _dbContext.Produit.FindAsync(produit.Id_Produit);
                    if (produitDb == null)
                    {
                        _logger.LogError("❌ Produit introuvable : {Id}", produit.Id_Produit);
                        continue;
                    }

                    var licence = await _licenceService.CreerLicenceAsync(commande, produitDb, user);
                    if (licence != null)
                    {
                        licences.Add(licence);
                        _logger.LogInformation("✅ Licence créée : {Cle} pour produit {Nom}", licence.Cle, produitDb.Nom);
                    }
                }

                // 📦 Lien de téléchargement fixe
                const string lienTelechargement = "https://mikroplus.dscloud.me:1998/sharing/sNjShnRnx";

                if (licences.Any() && !string.IsNullOrEmpty(user.Email))
                {
                    foreach (var licence in licences)
                    {
                        await _emailSender.SendEmailAsync(
                            user.Email,
                            "Votre Lizenzaktivierung",
                            $"Ihr Lizenzschlüssel: {licence.Cle}<br><a href='{lienTelechargement}'>Software herunterladen</a>");

                        _logger.LogInformation("📧 Email envoyé avec lien fixe pour licence : {Cle}", licence.Cle);
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("✅ Capture finalisée avec succès.");

                return new Order { Id = orderId, Status = "COMPLETED", Links = new List<LinkDescription>() };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "🚨 Exception durant la capture de commande : {OrderId}", orderId);
                return null;
            }
        }





        public async Task<string> GetPaymentStatusAsync(string orderId)
        {
            var request = new OrdersGetRequest(orderId);

            try
            {
                var response = await _paypalClient.Execute(request);
                var result = response.Result<Order>();

                _logger.LogInformation("🔍 Statut du paiement PayPal pour Order ID {OrderId} : {Status}", orderId, result.Status);

                return result.Status; // Peut être "APPROVED", "COMPLETED", "PENDING", etc.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la récupération du statut du paiement pour Order ID : {OrderId}", orderId);
                return "ERROR";
            }
        }
        public async Task<bool> CapturePaymentAsync(string orderId)
        {
            var captureRequest = new OrdersCaptureRequest(orderId);
            captureRequest.RequestBody(new OrderActionRequest());

            try
            {
                var response = await _paypalClient.Execute(captureRequest);
                var result = response.Result<Order>();

                if (result.Status == "COMPLETED")
                {
                    _logger.LogInformation("✅ Paiement capturé avec succès pour Order ID {OrderId}", orderId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("⚠️ Capture du paiement échouée. Statut : {Status}", result.Status);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la capture du paiement pour Order ID : {OrderId}");
                return false;
            }
        }


    }
}
