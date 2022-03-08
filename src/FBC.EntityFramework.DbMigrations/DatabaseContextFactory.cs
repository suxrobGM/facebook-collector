using FBC.EntityFramework.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace FBC.EntityFramework.DbMigrations;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = ConnectionStrings.Local;
        return new DatabaseContext(connectionString);
    }
}