using Microsoft.EntityFrameworkCore.Design;
using FBC.EntityFramework.Data;

namespace FBC.EntityFramework.DbMigrations;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = ConnectionStrings.Local;
        return new DatabaseContext(connectionString);
    }
}