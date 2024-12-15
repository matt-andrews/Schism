using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Schism.Hub.Migrations.Pgsql.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Registrations",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClientId = table.Column<string>(type: "text", nullable: false),
                Namespace = table.Column<string>(type: "text", nullable: false),
                Uri = table.Column<string>(type: "text", nullable: false),
                LastPing = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Version = table.Column<string>(type: "text", nullable: false),
                __ConnectionPointsBlob = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Registrations", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Registrations_ClientId",
            table: "Registrations",
            column: "ClientId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Registrations");
    }
}