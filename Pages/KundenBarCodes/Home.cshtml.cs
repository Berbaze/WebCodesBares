using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCodesBares.Data;
using Microsoft.EntityFrameworkCore;

namespace WebCodesBares.Pages.KundenBarCodes
{
    public class HomeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HomeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Produit> NeuesteProdukte { get; set; } = new();

        public async Task OnGetAsync()
        {
            NeuesteProdukte = await _context.Produit
                .OrderByDescending(p => p.Id_Produit)
                .Take(5) 
                .Select(p => new Produit 
                {
                    Id_Produit = p.Id_Produit,
                    Nom = p.Nom ?? "Unbekannt",  
                    Description = p.Description ?? "Keine Beschreibung",
                    Prix= p.Prix, 
                    ImageUrl = p.ImageUrl ?? "/images/default.webp" 
                })
                .ToListAsync();
        }
    }
}
