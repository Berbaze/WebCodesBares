using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebCodesBares.Data;
using WebCodesBares.Data.Models;

namespace WebCodesBares.Pages.KundenBarCodes
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }
      
        [BindProperty]
        public Kunden Kunde { get; set; } = new Kunden();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Kunde = await _context.Kunden.FindAsync(id);
            if (Kunde == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Attach(Kunde).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
