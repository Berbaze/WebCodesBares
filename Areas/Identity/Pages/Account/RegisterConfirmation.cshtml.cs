using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System;
using WebCodesBares.Data;
using Microsoft.AspNetCore.Identity.UI.Services; // ✅ correct interface

public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _env;

    public RegisterConfirmationModel(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _env = env;
    }

    public string Email { get; set; }
    public bool DisplayConfirmAccountLink { get; set; }
    public string EmailConfirmationUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (email == null)
            return RedirectToPage("/Index");

        returnUrl ??= Url.Content("~/");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"Impossible de charger l'utilisateur avec l'adresse e-mail '{email}'.");

        Email = email;

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        EmailConfirmationUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
            protocol: Request.Scheme)!;

        // 🛑 FORCER l'envoi de l'email peu importe l'environnement
        DisplayConfirmAccountLink = false;

        await _emailSender.SendEmailAsync(email,
            "Bestätigen Sie Ihr Konto",
            $"Hallo {user.Vorname},\n\n" +
              $"Vielen Dank für Ihre Registrierung auf unserer Website.\n\n" +
              $"Bitte bestätigen Sie Ihre E-Mail-Adresse, indem Sie auf den folgenden Link klicken:" +
              $"{EmailConfirmationUrl}Konto bestätigen\n\n" +
              $"Mit freundlichen Grüßen,\n\n" +
              $"Ihr Team von WebCodesBares");

        return Page();
    }

}
