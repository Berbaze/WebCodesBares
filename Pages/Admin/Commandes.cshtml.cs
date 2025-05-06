using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;

namespace WebCodesBares.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CommandesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public List<Commande>? ListeCommandes { get; set; }
        public CommandesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            ListeCommandes = await _context.Commande
                .Include(c => c.CommandeProduits)
                .ThenInclude(cp => cp.Produit)
                .Where(c => c.EstPaye)
                .OrderByDescending(c => c.DateAchat)
                .ToListAsync();
        }
    }
}
