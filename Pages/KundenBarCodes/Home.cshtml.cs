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
                .OrderByDescending(p => p.Id_Produit) // Tri par ID d�croissant pour les nouveaux produits
                .Take(5) // R�cup�re les 5 derniers produits
                .Select(p => new Produit // G�rer les valeurs NULL ici
                {
                    Id_Produit = p.Id_Produit,
                    Nom = p.Nom ?? "Unbekannt",  // Remplace NULL par "Unbekannt"
                    Description = p.Description ?? "Keine Beschreibung",
                    Prix= p.Prix, // Si NULL, remplace par 0
                    ImageUrl = p.ImageUrl ?? "/images/default.webp" // Image par d�faut si NULL
                })
                .ToListAsync();
        }
    }
}
