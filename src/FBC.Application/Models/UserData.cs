using System.Text.Json.Serialization;

namespace FBC.Application.Models;

internal class UserData
{
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }
}
