using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;
using WebCodesBares.Data.Service;
using WebCodesBares.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services; // Import interface Email

namespace WebCodesBares.Pages.Produits
{
    public class PanierModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PanierModel> _logger;
        private readonly IEmailSender _emailSender;  // Utilisation Interface
        private readonly UserManager<ApplicationUser> _userManager;
        public bool PanierEstVide { get; set; } = false;


        public PanierModel(ApplicationDbContext context,
                           ILogger<PanierModel> logger,
                           IEmailSender emailSender,  // Injection Interface
                           UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public List<Produit> ProduitsPanier { get; set; } = new List<Produit>();
        public decimal TotalPanier => ProduitsPanier.Sum(p => p.Prix);

        public async Task<IActionResult> OnGetAsync()
        {
            var panierIds = HttpContext.Session.GetObjectFromJson<List<int>>("Panier") ?? new List<int>();

            if (panierIds.Any())
            {
                ProduitsPanier = await _context.Produit
                                               .Where(p => panierIds.Contains(p.Id_Produit))
                                               .ToListAsync();

                _logger.LogInformation($"📦 Chargement du panier : {ProduitsPanier.Count} produits.");
            }
            else
            {
                _logger.LogInformation("⚠️ Panier vide lors du chargement.");
                PanierEstVide = true;
            }

            return Page();
        }


        public IActionResult OnPostSupprimer(int idProduit)
        {
            var panier = HttpContext.Session.GetObjectFromJson<List<int>>("Panier") ?? new List<int>();

            if (panier.Contains(idProduit))
            {
                panier.Remove(idProduit);
                HttpContext.Session.SetObjectAsJson("Panier", panier);
                _logger.LogInformation($"❌ Produit ID {idProduit} supprimé du panier.");
            }
            else
            {
                _logger.LogWarning($"⚠️ Tentative de suppression d'un produit non présent dans le panier : ID {idProduit}");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostValiderAchatAsync()
        {
            var panierIds = HttpContext.Session.GetObjectFromJson<List<int>>("Panier") ?? new List<int>();

            if (!panierIds.Any())
            {
                _logger.LogWarning("⚠️ Achat annulé : panier vide.");
                return RedirectToPage();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("⚠️ Utilisateur non connecté.");
                    return BadRequest("Vous devez être connecté pour valider l'achat.");
                }

                var commande = new Commande
                {
                    ClientId = user.Id,
                    DateAchat = DateTime.Now,
                    EstPaye = false
                };
                await _context.Commande.AddAsync(commande);
                await _context.SaveChangesAsync();

                var produits = await _context.Produit
                                             .Where(p => panierIds.Contains(p.Id_Produit))
                                             .ToListAsync();

                if (produits.Count != panierIds.Count)
                {
                    _logger.LogWarning("⚠️ Certains produits du panier ne sont pas trouvés en base !");
                    return BadRequest("Certains produits sont introuvables.");
                }

                var commandeProduits = produits.Select(produit => new CommandeProduit
                {
                    Id_Commande = commande.Id_Commande,
                    Id_Produit = produit.Id_Produit
                }).ToList();

                await _context.CommandeProduit.AddRangeAsync(commandeProduits);
                await _context.SaveChangesAsync();

                foreach (var produit in produits)
                {
                    string cleLicence = LicenceService.GenererCle();

                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        string lienGoogleDrive = "https://drive.google.com/uc?export=download&id=14qan5tY8jIz0ORx72Y-biR7MsNTBeDL2";

                        await _emailSender.SendEmailAsync(
                            user.Email,
                            "Votre Licence WebCodesBares",
                            $@"Bonjour {user.UserName},

Merci pour votre achat !

🔑 Licence : {cleLicence}

📥 Télécharger l'application : {lienGoogleDrive}

Cordialement,  
L'équipe WebCodesBares"
                        );
                    }
                }

                await transaction.CommitAsync();

                HttpContext.Session.SetObjectAsJson("Panier", new List<int>());

                _logger.LogInformation("✅ Commande validée avec succès ! ID Commande : {CommandeId}", commande.Id_Commande);

                return RedirectToPage("/Comande/Confirmation", new { orderId = commande.Id_Commande });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"🚨 ERREUR : {ex.Message} \n {ex.StackTrace}");
                return BadRequest("Une erreur est survenue lors de la validation de l'achat.");
            }
        }
    }
}
