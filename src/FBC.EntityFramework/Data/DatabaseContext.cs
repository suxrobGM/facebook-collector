using FBC.EntityFramework.Helpers;

namespace FBC.EntityFramework.Data
{
    public class DatabaseContext : DbContext
    {
        private readonly string connectionString;

        public DatabaseContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            connectionString = ConnectionStrings.Local;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                DbContextHelpers.ConfigureMySql(connectionString, options);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.HasOne(m => m.Hometown)
                    .WithOne()
                    .HasForeignKey<User>(m => m.HometowId);

                entity.HasOne(m => m.CurrentCity)
                    .WithOne()
                    .HasForeignKey<User>(m => m.CurrentCityId);

                entity.HasMany(m => m.WorkPlaces)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.Contacts)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.Educations)
                    .WithMany(m => m.Students);

                entity.HasMany(m => m.Skills)
                    .WithMany(m => m.Users);

                entity.HasMany(m => m.Languages)
                    .WithMany(m => m.Speakers);

                entity.HasMany(m => m.LivedCities)
                    .WithMany(m => m.People);
            });

            builder.Entity<Company>(entity =>
            {
                entity.HasMany(m => m.Employees)
                    .WithOne(m => m.Company)
                    .HasForeignKey(m => m.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
