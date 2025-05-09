using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;

namespace WebCodesBares.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class GererAdminsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GererAdminsModel> _logger;

        public GererAdminsModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<GererAdminsModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        public class UserWithRole
        {
            public ApplicationUser User { get; set; } = null!;
            public bool EstAdmin { get; set; }
        }

        public List<UserWithRole> UtilisateursAvecRole { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.Email != null && u.UserName != null)
                    .ToListAsync();

                foreach (var user in users)
                {
                    var estAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    UtilisateursAvecRole.Add(new UserWithRole
                    {
                        User = user,
                        EstAdmin = estAdmin
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ FEHLER beim Laden der Benutzer: " + ex.Message);
                UtilisateursAvecRole = new();
            }
        }

        public async Task<IActionResult> OnPostPromouvoirAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                await _userManager.AddToRoleAsync(user, "Admin");

                _context.AuditLogs.Add(new AuditLog
                {
                    Action = $"[BEFÖRDERUNG] {user.Email} wurde zum Admin ernannt.",
                    EffectuePar = User.Identity?.Name ?? "System",
                    Date = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRetrograderAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");

                _context.AuditLogs.Add(new AuditLog
                {
                    Action = $"[HERABSTUFUNG] {user.Email} ist kein Admin mehr.",
                    EffectuePar = User.Identity?.Name ?? "System",
                    Date = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSupprimerAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);

                    _context.AuditLogs.Add(new AuditLog
                    {
                        Action = $"[LÖSCHUNG] Benutzer gelöscht: {user.Email}",
                        EffectuePar = User.Identity?.Name ?? "System",
                        Date = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Fehler beim Löschen des Benutzers mit ID {userId}");
                ModelState.AddModelError(string.Empty, "Fehler beim Löschen des Benutzers.");
                return Page();
            }
        }






    }
}
