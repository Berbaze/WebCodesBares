using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebCodesBares.Data;

namespace WebCodesBares.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "E-Mail ist erforderlich.")]
            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
            [Display(Name = "E-Mail")]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Passwort ist erforderlich.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Das Passwort muss mindestens 6 Zeichen lang sein.")]
            [DataType(DataType.Password)]
            [Display(Name = "Passwort")]
            public string? Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Passwort bestätigen")]
            [Compare("Password", ErrorMessage = "Die Passwörter stimmen nicht überein.")]
            public string? ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Vorname ist erforderlich.")]
            [Display(Name = "Vorname")]
            public string? Vorname { get; set; }

            [Required(ErrorMessage = "Nachname ist erforderlich.")]
            [Display(Name = "Nachname")]
            public string? Nachname { get; set; }

            [Required(ErrorMessage = "Geburtsdatum ist erforderlich.")]
            [DataType(DataType.Date)]
            [Display(Name = "Geburtsdatum")]
            public DateTime Geburtsdatum { get; set; }

            [Phone(ErrorMessage = "Ungültige Telefonnummer.")]
            [Display(Name = "Telefonnummer")]
            public string? PhoneNumber { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.Vorname = Input.Vorname;
                user.Nachname = Input.Nachname;
                user.PhoneNumber = Input.PhoneNumber;
                user.Geburtsdatum = Input.Geburtsdatum;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Ein neuer Benutzer wurde mit einem Passwort erstellt.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    // Générer un Token sécurisé pour confirmation email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Encoder le token pour pouvoir l'envoyer par URL
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // Générer le lien de confirmation
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Envoi de l'email
                    await _emailSender.SendEmailAsync(
              Input.Email,
              "Bestätigen Sie Ihr Konto",
              $"Hallo {user.Vorname},<br><br>" +
              $"Vielen Dank für Ihre Registrierung auf unserer Website.<br><br>" +
              $"Bitte bestätigen Sie Ihre E-Mail-Adresse, indem Sie auf den folgenden Link klicken:<br><br>" +
              $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Konto bestätigen</a><br><br>" +
              $"Mit freundlichen Grüßen,<br>" +
              $"Ihr Team von WebCodesBares");

                    // Si confirmation obligatoire → On redirige vers page "confirmation envoyée"
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });

                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return new ApplicationUser();
            }
            catch
            {
                throw new InvalidOperationException($"Eine Instanz von '{nameof(ApplicationUser)}' konnte nicht erstellt werden.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Das Identity-System erfordert einen UserStore mit E-Mail-Verwaltung.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
