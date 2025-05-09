using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCodesBares.Data.Models;
using WebCodesBares.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace WebCodesBares.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class LicencesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LicencesModel> _logger;


        public List<Licence> ListeLicences { get; set; } = new();

        public LicencesModel(ApplicationDbContext context, ILogger<LicencesModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            ListeLicences = await _context.Licence
                .OrderByDescending(l => l.DateEmission)
                .ToListAsync();
        }

        // ✅ Désactivation avec EstSuspendue + DatePause
        public async Task<IActionResult> OnPostDesactiverAsync(string cle)
        {
            var licence = await _context.Licence.FirstOrDefaultAsync(l => l.Cle == cle);
            if (licence != null)
            {
                licence.EstSuspendue = true;
                licence.DatePause = DateTime.UtcNow;
                licence.Active = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // ✅ Export CSV des licences
        public async Task<FileResult> OnPostExporterAsync()
        {
            var licences = await _context.Licence
                .OrderByDescending(l => l.DateEmission)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Clé,Type,Utilisateur,Email,DateEmission,DateExpiration,Active,Suspendue");

            foreach (var l in licences)
            {
                sb.AppendLine($"{l.Cle},{l.Type},{l.UserName},{l.Email},{l.DateEmission:yyyy-MM-dd},{l.DateExpiration:yyyy-MM-dd},{(l.Active ? "Oui" : "Non")},{(l.EstSuspendue ? "Oui" : "Non")}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "Licences.csv");
        }
        public async Task<IActionResult> OnPostReactiverAsync(string cle)
        {
            var licence = await _context.Licence.FirstOrDefaultAsync(l => l.Cle == cle);
            if (licence != null && licence.EstSuspendue)
            {
                licence.EstSuspendue = false;
                licence.DatePause = null;
                licence.Active = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostSupprimerAsync(string cle)
        {
            var licence = await _context.Licence.FirstOrDefaultAsync(l => l.Cle == cle);
            if (licence != null)
            {
                try
                {
                    // 🔻 Désactiver temporairement le trigger pour éviter le blocage EF
                    await _context.Database.ExecuteSqlRawAsync("DISABLE TRIGGER trg_DeleteCascade_Licence ON Licence");

                    _context.Licence.Remove(licence);

                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = $"🗑️ Lizenz löschen ({licence.Type})",
                        EffectuePar = User.Identity?.Name ?? "Système",
                        Date = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();

                    // 🔺 Réactiver le trigger
                    await _context.Database.ExecuteSqlRawAsync("ENABLE TRIGGER trg_DeleteCascade_Licence ON Licence");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erreur lors de la suppression de la licence.");
                    TempData["Error"] = "❌ Ein Fehler ist aufgetreten beim Löschen.";
                }
            }

            return RedirectToPage();
        }

    }
}
