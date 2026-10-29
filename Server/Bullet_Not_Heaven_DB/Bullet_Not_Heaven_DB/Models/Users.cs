using System.ComponentModel.DataAnnotations;

namespace Bullet_Not_Heaven_DB.Models
{
    public class Score
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SurvivalTimeInSeconds { get; set; }

        [Required]
        public int EnemiesKilled { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}