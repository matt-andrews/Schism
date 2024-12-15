using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Schism.Hub.Migrations.Pgsql")]
namespace Schism.Hub.Data;

internal class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<RegistrationTable> Registrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        RegistrationTable.OnModelCreating(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}