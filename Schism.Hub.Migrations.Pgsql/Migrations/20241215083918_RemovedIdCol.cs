using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Schism.Hub.Migrations.Pgsql.Migrations;

/// <inheritdoc />
public partial class RemovedIdCol : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Registrations",
            table: "Registrations");

        migrationBuilder.DropIndex(
            name: "IX_Registrations_ClientId",
            table: "Registrations");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "Registrations");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Registrations",
            table: "Registrations",
            column: "ClientId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropPrimaryKey(
            name: "PK_Registrations",
            table: "Registrations");

        migrationBuilder.AddColumn<int>(
            name: "Id",
            table: "Registrations",
            type: "integer",
            nullable: false,
            defaultValue: 0)
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        migrationBuilder.AddPrimaryKey(
            name: "PK_Registrations",
            table: "Registrations",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Registrations_ClientId",
            table: "Registrations",
            column: "ClientId");
    }
}