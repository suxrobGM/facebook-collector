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

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Institution> Institutions { get; set; }
        public virtual DbSet<Company> Companies { get; set; }

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
                entity.HasMany(m => m.Works)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);                             
            });

            builder.Entity<UserInstitution>(entity =>
            {
                entity.HasKey(k => new { k.StudentId, k.InstitutionId });

                entity.HasOne(m => m.Institution)
                    .WithMany(m => m.Students)
                    .HasForeignKey(m => m.InstitutionId);

                entity.HasOne(m => m.Student)
                    .WithMany(m => m.Institutions)
                    .HasForeignKey(m => m.StudentId);                
            });

            builder.Entity<Company>(entity =>
            {
                entity.HasMany(m => m.Employees)
                    .WithOne(m => m.Company)
                    .HasForeignKey(m => m.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
            });
        }
    }
}
