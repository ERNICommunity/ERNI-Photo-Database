using ERNI.PhotoDatabase.DataAccess.DomainModel;
using ERNI.PhotoDatabase.DataAccess.EntityConfiguration;
using Microsoft.EntityFrameworkCore;

namespace ERNI.PhotoDatabase.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<PhotoTag> PhotoTag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PhotoConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new PhotoTagConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}
