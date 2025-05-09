using Microsoft.AspNetCore.Mvc;
using WebCodesBares.Data.Service;

namespace WebCodesBares.Controllers
{

    [ApiController]
    [Route("api/synology")]
    public class SynologyController : Controller
    {
        private readonly SynologyAuthService _authService;
        private readonly SynologyShareService _shareService;


        public SynologyController(SynologyAuthService authService, SynologyShareService shareService)
        {
            _authService = authService;
            _shareService = shareService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            string sid = await _authService.LoginAsync("Archivcode", "mp1993#Zugang");
            return Ok(sid);

        }
        [HttpPost("share")]
        public async Task<IActionResult> CreateShare([FromQuery] string path)
        {
            try
            {
                string sid = await _authService.LoginAsync("Archivcode", "mp1993#Zugang");

                string shareUrl = await _shareService.CreateShareLinkAsync(sid, path);
                return Ok(new { share_url = shareUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
