namespace FBC.Domain;

internal static class Generator
{
    public static string NewGuid() 
        => Guid.NewGuid().ToString();
}