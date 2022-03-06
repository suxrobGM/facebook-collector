using FBC.DbMigrator;
using FBC.EntityFramework;
using FBC.EntityFramework.Data;

var connectionString = ConnectionStrings.Local;

Console.WriteLine("Connection string: " + connectionString);
Console.WriteLine("Initializing database...");

await SeedData.InitializeAsync(new DatabaseContext(connectionString));

Console.WriteLine("Finished!");
Console.ReadLine();