using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bullet_Not_Heaven_DB.Data;
using Bullet_Not_Heaven_DB.Models;

namespace Bullet_Not_Heaven_DB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Klasa pomocnicza (DTO) określająca, jakie dane serwer ma przyjąć z Unity/silnika gry
        public class AuthRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            // Sprawdzenie, czy użytkownik o takiej nazwie już istnieje w bazie
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Użytkownik o takiej nazwie już istnieje.");
            }

            var user = new User
            {
                Username = request.Username,
                // W uproszczonej wersji edukacyjnej zapisujemy hasło wprost. 
                // W docelowym projekcie warto dodać tu funkcję szyfrującą (tzw. hash).
                PasswordHash = request.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Konto zostało pomyślnie utworzone." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            // Szukanie użytkownika o podanym loginie
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Weryfikacja loginu i hasła
            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");
            }

            return Ok(new { message = "Zalogowano pomyślnie.", userId = user.Id });
        }
    }
}