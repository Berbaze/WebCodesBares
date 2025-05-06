using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;

namespace WebCodesBares.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public List<UserStats> Stats { get; set; } = new();
        public List<AuditEntry> Logs { get; set; } = new();
        public class UserStats
        {
            public string Email { get; set; } = string.Empty;
            public int NombreLicences { get; set; }
            public DateTime? DerniereLicence { get; set; }
        }

        public class AuditEntry
        {
            public string Action { get; set; } = string.Empty;
            public string EffectuePar { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }
        public async Task OnGetAsync()
        {
            Stats = await _context.Users
                .Select(u => new UserStats
                {
                    Email = u.Email,
                    NombreLicences = _context.Licence.Count(l => l.Email == u.Email),
                    DerniereLicence = _context.Licence
                        .Where(l => l.Email == u.Email)
                        .OrderByDescending(l => l.DateEmission)
                        .Select(l => (DateTime?)l.DateEmission)
                        .FirstOrDefault()
                })
                .ToListAsync();

            Logs = await _context.AuditLogs
                .OrderByDescending(a => a.Date)
                .Take(50)
                .Select(a => new AuditEntry
                {
                    Action = a.Action,
                    EffectuePar = a.EffectuePar,
                    Date = a.Date
                })
                .ToListAsync();
        }
    }
}

