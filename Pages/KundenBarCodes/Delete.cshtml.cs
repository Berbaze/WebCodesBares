using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;

namespace WebCodesBares.Pages.KundenBarCodes
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Kunden Kunde { get; set; } = new Kunden();

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var kunde = await _context.Kunden.FindAsync(id);
            if (kunde == null) return NotFound();

            _context.Kunden.Remove(kunde);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
