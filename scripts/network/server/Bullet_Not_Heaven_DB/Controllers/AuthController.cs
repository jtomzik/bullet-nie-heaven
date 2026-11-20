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

        public class AuthRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Użytkownik o takiej nazwie już istnieje.");
            }

            var user = new User
            {
                Username = request.Username,
                // Bezpieczne haszowanie hasła przed zapisem do bazy
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Konto zostało pomyślnie utworzone." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Weryfikacja loginu oraz zgodności skrótu hasła
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Nieprawidłowa nazwa użytkownika lub hasło.");
            }

            // Generowanie prostego tokenu sesji (w przyszłości można tu wpiąć pełne JWT)
            var token = Guid.NewGuid().ToString();

            return Ok(new { message = "Zalogowano pomyślnie.", userId = user.Id, token = token });
        }
    }
}