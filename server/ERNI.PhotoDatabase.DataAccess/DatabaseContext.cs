using Microsoft.EntityFrameworkCore;

namespace ERNI.PhotoDatabase.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }
    }
}
