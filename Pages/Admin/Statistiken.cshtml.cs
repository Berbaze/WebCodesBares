using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;

namespace WebCodesBares.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class statistiken : PageModel
    {
        private readonly ApplicationDbContext _context;
        public Dictionary<string, int> CommandesParMois { get; set; } = new();

        public statistiken(ApplicationDbContext context)
        {
            _context = context;
        }

        public Dictionary<string, int> LicencesParType { get; set; } = new();
        public List<MoisCommande> CommandesMois { get; set; } = new();

        public class MoisCommande
        {
            public string Mois { get; set; } = "";
            public int Count { get; set; }
        }

        public async Task OnGetAsync()
        {
            // 📊 Licences par type
            LicencesParType = await _context.Licence
                .GroupBy(l => l.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            // 📅 Commandes par mois (LINQ to Objects pour gérer le ToString D2)
            CommandesMois = _context.Commande
                .AsEnumerable() // LINQ côté mémoire (client)
                .GroupBy(c => new { c.DateAchat.Year, c.DateAchat.Month })
                .Select(g => new MoisCommande
                {
                    Mois = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(g => g.Mois)
                .ToList(); // ← ⚠️ doit être synchrone ici car LINQ en mémoire
        }
    }
}
