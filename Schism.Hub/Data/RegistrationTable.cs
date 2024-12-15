using Microsoft.EntityFrameworkCore;
using Schism.Hub.Abstractions.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Schism.Hub.Data;

public class RegistrationTable
{
    public required string ClientId { get; set; }
    public required string Namespace { get; set; }
    public required string Uri { get; set; }
    public required DateTimeOffset LastPing { get; set; }
    public required string Version { get; set; }

    [NotMapped]
    public ConnectionPoint[] ConnectionPoints { get; set; } = [];
    [JsonIgnore]
#pragma warning disable IDE1006 // Naming Styles
    public string __ConnectionPointsBlob
#pragma warning restore IDE1006 // Naming Styles
    {
        get => JsonSerializer.Serialize(ConnectionPoints);
        set => ConnectionPoints = JsonSerializer.Deserialize<ConnectionPoint[]>(value ?? "") ?? [];
    }

    public static ModelBuilder OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RegistrationTable>()
            .HasKey(nameof(ClientId));

        return builder;
    }
}