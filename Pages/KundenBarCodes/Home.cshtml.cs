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
                .OrderByDescending(p => p.Id_Produit) // Tri par ID décroissant pour les nouveaux produits
                .Take(5) // Récupère les 5 derniers produits
                .Select(p => new Produit // Gérer les valeurs NULL ici
                {
                    Id_Produit = p.Id_Produit,
                    Nom = p.Nom ?? "Unbekannt",  // Remplace NULL par "Unbekannt"
                    Description = p.Description ?? "Keine Beschreibung",
                    Prix= p.Prix, // Si NULL, remplace par 0
                    ImageUrl = p.ImageUrl ?? "/images/default.webp" // Image par défaut si NULL
                })
                .ToListAsync();
        }
    }
}
