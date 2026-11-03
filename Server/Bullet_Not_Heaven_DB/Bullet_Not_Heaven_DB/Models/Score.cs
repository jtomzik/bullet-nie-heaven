using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bullet_Not_Heaven_DB.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Score> Scores { get; set; } = new List<Score>();
    }
}