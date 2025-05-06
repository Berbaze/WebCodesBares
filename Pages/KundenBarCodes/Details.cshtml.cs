using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;


namespace WebCodesBares.Pages.KundenBarCodes
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public Kunden Kunde { get; set; } = new Kunden();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Kunde = await _context.Kunden.FindAsync(id);
            if (Kunde == null) return NotFound();
            return Page();
        }
    }
}
