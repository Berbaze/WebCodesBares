using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebCodesBares.Data;

namespace WebCodesBares.Pages.KundenBarCodes
{
    public class MeineDatenModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;


        public MeineDatenModel(
     UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager
 )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Vorname")]
            public string? Vorname { get; set; }

            [Required]
            [Display(Name = "Nachname")]
            public string? Nachname { get; set; }

            [Display(Name = "E-Mail")]
            public string? Email { get; set; }

            [Display(Name = "Telefon")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Geburtsdatum")]
            [DataType(DataType.Date)]
            public DateTime? Geburtsdatum { get; set; }
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Neues Passwort")]
            public string? NewPassword { get; set; }
            [DataType(DataType.Password), Display(Name = "Passwort bestätigen")]
            [Compare(nameof(NewPassword), ErrorMessage = "❌ Die Passwörter stimmen nicht überein.")]
            public string? ConfirmPassword { get; set; }
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            Input = new InputModel
            {
                Vorname = user.Vorname,
                Nachname = user.Nachname,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Geburtsdatum = user.Geburtsdatum
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Vorname = Input.Vorname;
            user.Nachname = Input.Nachname;
            user.PhoneNumber = Input.PhoneNumber;
            user.Geburtsdatum = Input.Geburtsdatum;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);

                if (!passResult.Succeeded)
                {
                    foreach (var error in passResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return Page();
                }
            }

            TempData["Message"] = "✅ Änderungen wurden gespeichert. Du wirst neu angemeldet.";
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

    }
}
