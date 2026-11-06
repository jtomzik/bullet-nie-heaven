using Microsoft.EntityFrameworkCore;
using Bullet_Not_Heaven_DB.Models;

namespace Bullet_Not_Heaven_DB.Data
{
    public class AppDbContext : DbContext
    {
        // Konstruktor wstrzykujący konfigurację (m.in. informację o użyciu SQLite)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Reprezentacja tabel, które powstaną w bazie danych
        public DbSet<User> Users { get; set; }
        public DbSet<Score> Scores { get; set; }
    }
}