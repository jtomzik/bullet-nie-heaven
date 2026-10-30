using Microsoft.AspNetCore.Mvc;

namespace Bullet_Not_Heaven_DB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET: /api/health/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            // Zwraca szybki status 200 OK z krótką informacją
            return Ok(new { status = "online", message = "Serwer Bullet Heaven jest aktywny." });
        }
    }
}