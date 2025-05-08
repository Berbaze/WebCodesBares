using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCodesBares.Pages
{
    public class GateModel : PageModel
    {
        [BindProperty]
        public string? Passcode { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            const string correctPassword = "microplus"; // 🔐 Ton mot de passe ici

            if (Passcode == correctPassword)
            {
                HttpContext.Session.SetString("AccessGranted", "true");
                return RedirectToPage("/Index"); // Page de démarrage après accès
            }

            ErrorMessage = "Falsches Passwort.";
            return Page();
        }
    }
}
