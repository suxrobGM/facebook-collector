﻿namespace FBC.EntityFramework.Helpers;

internal static class DbContextHelpers
{
    public static void ConfigureMySql(string connectionString, DbContextOptionsBuilder options)
    {
        options.UseMySql(connectionString,
                ServerVersion.AutoDetect(connectionString),
                o =>
                {
                    o.MigrationsAssembly("FBC.EntityFramework.DbMigrations");
                    o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                })
            .UseLazyLoadingProxies();
    }
}