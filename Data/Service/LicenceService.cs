using System;
using System.Security.Cryptography;
using System.Text;
using WebCodesBares.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace WebCodesBares.Data.Service
{
    public class LicenceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LicenceService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LicenceService(
            ApplicationDbContext dbContext,
            ILogger<LicenceService> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public static string GenererCle()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }

        public static string GenererMotDePasseTemporaire()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(caracteres, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Crée et enregistre une licence pour une commande donnée.
        /// </summary>
      public async Task<Licence> CreerLicenceAsync(Commande commande, Produit produit, ApplicationUser user)
{
    if (!Enum.TryParse<TypeLicence>(produit.Type, ignoreCase: true, out var type))
    {
        _logger.LogError("❌ Type de licence invalide : {Type}", produit.Type);
        throw new Exception($"Type de licence invalide : {produit.Type}");
    }

    var config = LicenceConfiguration.GetConfiguration(type);

    // 🛡️ FIX: Prüfe ob Lizenz für diese Commande + Produit + User schon existiert
    var existing = await _dbContext.Licence.FirstOrDefaultAsync(l =>
        l.Id_Commande == commande.Id_Commande &&
        l.UserId == user.Id &&
        l.Type == type.ToString());

    if (existing != null)
    {
        _logger.LogWarning("⚠️ Lizenz existiert bereits für Nutzer: {User} | Produit: {Produit} | Cmd: {CommandeId}", user.Email, produit.Nom, commande.Id_Commande);
        return existing;
    }

    // 🧼 Alte Lizenzen deaktivieren
    var anciennesLicences = await _dbContext.Licence
        .Where(l => l.Email == user.Email)
        .ToListAsync();

    foreach (var oldLicence in anciennesLicences)
    {
        if (oldLicence.BarcodesRestants <= 0 || oldLicence.DateExpiration <= DateTime.UtcNow)
        {
            oldLicence.Active = false;
        }
    }

    string cle = Guid.NewGuid().ToString("N").ToUpper();

    var licence = new Licence
    {
        Cle = cle,
        Type = type.ToString(),
        NombreUtilisateurs = config.NombreUtilisateurs,
        NombreBarcodes = config.NombreBarcodes,
        Prix = config.Prix,
        PrixMaintenance = config.PrixMaintenance,
        DateEmission = DateTime.UtcNow,
        DateExpiration = DateTime.UtcNow.AddMonths(config.DureeValiditeEnMois),
        Active = true,
        Id_Commande = commande.Id_Commande,
        UserId = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        BarcodesRestants = config.NombreBarcodes
    };

    await _dbContext.Licence.AddAsync(licence);

    _dbContext.AuditLogs.Add(new AuditLog
    {
        Action = $"[ACHAT LICENCE] Type: {licence.Type}, Clé: {licence.Cle}",
        EffectuePar = user.Email,
        Date = DateTime.UtcNow
    });

    await _dbContext.SaveChangesAsync();

    // 🛡️ Ensure Role
    var adminRole = await _roleManager.FindByNameAsync("SuperUser");
    if (adminRole == null)
    {
        adminRole = new IdentityRole("SuperUser");
        await _roleManager.CreateAsync(adminRole);
    }

    var existingUser = await _userManager.FindByIdAsync(user.Id);
    if (existingUser == null)
        throw new Exception("Utilisateur introuvable.");

    if (!await _userManager.IsInRoleAsync(existingUser, "SuperUser"))
    {
        await _userManager.AddToRoleAsync(existingUser, "SuperUser");
        _logger.LogInformation("👑 User {UserName} → Rolle SuperUser hinzugefügt", user.UserName);
    }

    _logger.LogInformation("✅ Neue Lizenz erstellt → {Cle} für {UserName}", cle, user.UserName);
    return licence;
}



        /// <summary>
        /// Vérifie si une licence est toujours valide.
        /// </summary>
        public async Task<bool> VerifierLicenceAsync(string cle)
        {
            _logger.LogInformation("🔍 Vérification de la licence : {Cle}", cle);

            var licence = await _dbContext.Licence
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Cle == cle && l.Active);

            if (licence == null)
            {
                _logger.LogWarning("⚠️ Licence introuvable ou inactive : {Cle}", cle);
                return false;
            }

            bool estValide = licence.DateExpiration > DateTime.UtcNow;
            _logger.LogInformation("🟢 Licence {Cle} est valide : {EstValide}", cle, estValide);

            return estValide;
        }

        public async Task<Licence?> GetLicenceDetailsAsync(string cle)
        {
            return await _dbContext.Licence
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Cle == cle);
        }
        /// <summary>
        /// Récupère la première licence valide (non expirée, active, avec BarcodesRestants > 0)
        /// </summary>
        public async Task<Licence> GetLicenceDisponibleAsync(string email)
        {
            var licences = await _dbContext.Licence
                .Where(l => l.Email == email && l.DateExpiration > DateTime.UtcNow)
                .OrderByDescending(l => l.DateEmission) // Prend la PLUS RÉCENTE d'abord
                .ToListAsync();

            Licence? selectedLicence = null;

            foreach (var licence in licences)
            {
                if (licence.BarcodesRestants > 0 && licence.Active && !licence.EstSuspendue)
                {
                    selectedLicence = licence;
                    break;
                }
                else
                {
                    licence.Active = false; // désactive les autres
                }
            }

            await _dbContext.SaveChangesAsync();

            if (selectedLicence != null)
            {
                return selectedLicence;
            }

            throw new Exception("❌ Aucune licence disponible pour cet utilisateur.");
        }



        /// <summary>
        /// Consomme un code-barres d'une licence disponible.
        /// </summary>
        public async Task ConsommerBarcodeAsync(string email)
        {
            var licence = await GetLicenceDisponibleAsync(email);

            if (licence.BarcodesRestants <= 0)
            {
                _logger.LogWarning("🚫 Tentative de consommation sur une licence épuisée : {Cle}", licence.Cle);
                throw new Exception("Licence épuisée.");
            }

            licence.BarcodesRestants--;

            _logger.LogInformation("📉 Consommation d'un barcode. Reste : {Restants} pour la licence {Cle}", licence.BarcodesRestants, licence.Cle);

            await _dbContext.SaveChangesAsync();
        }
        public async Task<Licence> ConsommerEtRetournerLicenceAsync(string email)
        {
            var licences = await _dbContext.Licence
                .Where(l => l.Email == email && l.DateExpiration > DateTime.UtcNow)
                .OrderBy(l => l.DateEmission)
                .ToListAsync();

            foreach (var licence in licences)
            {
                if (!licence.Active || licence.EstSuspendue || licence.BarcodesRestants <= 0)
                {
                    licence.Active = false;
                    continue;
                }

                licence.BarcodesRestants--;
                _logger.LogInformation("📉 Décrément Licence {Cle} → Restants: {Restants}", licence.Cle, licence.BarcodesRestants);
                await _dbContext.SaveChangesAsync();
                return licence;
            }

            await _dbContext.SaveChangesAsync(); // enregistre désactivations
            throw new Exception("❌ Aucune licence disponible.");
        }


    }
}
