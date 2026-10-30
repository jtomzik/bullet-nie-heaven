using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bullet_Not_Heaven_DB.Data;
using Bullet_Not_Heaven_DB.Models;

namespace Bullet_Not_Heaven_DB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScoreController(AppDbContext context)
        {
            _context = context;
        }

        // DTO określające, co gra wyśle po zakończeniu rundy
        public class ScoreRequest
        {
            public int UserId { get; set; }
            public int SurvivalTimeInSeconds { get; set; }
            public int EnemiesKilled { get; set; }
        }

        // POST: /api/score
        // Endpoint wywoływany przez grę po ekranie Game Over
        [HttpPost]
        public async Task<IActionResult> SubmitScore([FromBody] ScoreRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return NotFound("Nie znaleziono użytkownika o podanym ID.");
            }

            var score = new Score
            {
                UserId = request.UserId,
                SurvivalTimeInSeconds = request.SurvivalTimeInSeconds,
                EnemiesKilled = request.EnemiesKilled
            };

            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Wynik zapisany na serwerze." });
        }

        // GET: /api/score/top10
        // Endpoint wywoływany przez grę w Menu Głównym
        [HttpGet("top10")]
        public async Task<IActionResult> GetTop10()
        {
            var topScores = await _context.Scores
                .Include(s => s.User) // Pobieramy też dane z tabeli User, żeby mieć nazwę gracza
                .OrderByDescending(s => s.SurvivalTimeInSeconds) // Sortujemy malejąco po czasie przetrwania
                .ThenByDescending(s => s.EnemiesKilled) // W przypadku remisu czasowego decydują zabicia
                .Take(10) // Zgodnie z wymogami pobieramy tylko TOP 10 najlepszych wyników
                .Select(s => new
                {
                    Username = s.User!.Username,
                    SurvivalTimeInSeconds = s.SurvivalTimeInSeconds,
                    EnemiesKilled = s.EnemiesKilled
                })
                .ToListAsync();

            return Ok(topScores);
        }
    }
}