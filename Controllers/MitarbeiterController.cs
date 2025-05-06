using Microsoft.AspNetCore.Mvc;
using WebCodesBares.Data.Models;
using WebCodesBares.Data;
using Microsoft.EntityFrameworkCore;

namespace WebCodesBares.Controllers
{
    [ApiController]
    [Route("api/mitarbeiter")]
    public class MitarbeiterController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public MitarbeiterController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("ajouter")]
        public async Task<IActionResult> AjouterMitarbeiter([FromBody] Mitarbeiter employe)
        {
            var admin = await _dbContext.Users.FindAsync(employe.AdminId);
            if (admin == null)
            {
                return BadRequest("Administrateur introuvable.");
            }

            var licence = await _dbContext.Licence.FirstOrDefaultAsync(l => l.UserId == employe.AdminId && l.Active);
            if (licence == null)
            {
                return BadRequest("Licence invalide.");
            }

            int maxEmployes = licence.Type switch
            {
                "Basic" => 1,
                "Pro" => 3,
                "Enterprise" => 10,
                _ => 0
            };

            int employesActuels = await _dbContext.Mitarbeiter.CountAsync(m => m.AdminId == employe.AdminId);

            if (employesActuels >= maxEmployes)
            {
                return BadRequest("Nombre maximum d'employés atteint pour cette licence.");
            }

            _dbContext.Mitarbeiter.Add(employe);
            await _dbContext.SaveChangesAsync();

            return Ok("Employé ajouté avec succès !");
        }
    }
}
