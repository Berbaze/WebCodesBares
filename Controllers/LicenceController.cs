using Microsoft.AspNetCore.Mvc;
using WebCodesBares.Data;
using WebCodesBares.Data.Service;

namespace WebCodesBares.Controllers
{
    [ApiController]
    [Route("api/licences")]
    public class LicenceController : ControllerBase
    {
        private readonly LicenceService _licenceService;
        private readonly ApplicationDbContext _dbContext;

        public LicenceController(LicenceService licenceService , ApplicationDbContext dbContext)
        {
            _licenceService = licenceService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Vérifie et retourne les détails d'une licence via sa clé.
        /// </summary>
        [HttpGet("verifier")]
        public async Task<IActionResult> VerifierLicence([FromQuery] string cle)
        {
            var licence = await _licenceService.GetLicenceDetailsAsync(cle);

            if (licence == null || licence.DateExpiration <= DateTime.UtcNow)
            {
                return NotFound(new { message = "Licence invalide ou expirée !" });
            }

            return Ok(new
            {
                licence.Cle,
                licence.Type,
                licence.NombreUtilisateurs,
                licence.NombreBarcodes,
                licence.DateExpiration
            });
        }
        /// <summary>
        /// Consomme un code-barres à partir de la première licence disponible.
        /// </summary>
        [HttpPost("consommer")]
        public async Task<IActionResult> ConsommerLicence([FromQuery] string email)
        {
            try
            {
                var licence = await _licenceService.GetLicenceDisponibleAsync(email);

                if (licence == null)
                    return BadRequest(new { message = "❌ Aucune licence disponible pour cet utilisateur." });

                if (!licence.Active || licence.EstSuspendue)
                    return BadRequest(new { message = "⛔ Cette licence est désactivée ou suspendue." });

                if (licence.DateExpiration <= DateTime.UtcNow)
                    return BadRequest(new { message = "📅 Cette licence a expiré." });

                if (licence.BarcodesRestants <= 0)
                    return BadRequest(new { message = "🚫 Aucun barcode restant sur cette licence." });

                licence = await _licenceService.ConsommerEtRetournerLicenceAsync(email);

                return Ok(new
                {
                    message = "✅ Code-barres consommé.",
                    cle = licence.Cle,
                    barcodesRestants = licence.BarcodesRestants
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "❌ Erreur : " + ex.Message });
            }
        }




    }
}
