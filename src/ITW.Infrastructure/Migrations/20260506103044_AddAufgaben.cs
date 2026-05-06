using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITW.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAufgaben : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aufgaben",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bereich = table.Column<int>(type: "int", nullable: false),
                    Titel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Prioritaet = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Quelle = table.Column<int>(type: "int", nullable: false),
                    SystemSchluessel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Faelligkeitsdatum = table.Column<DateOnly>(type: "date", nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErledigtAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aufgaben", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aufgaben_Bereich_Status",
                table: "Aufgaben",
                columns: new[] { "Bereich", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Aufgaben_SystemSchluessel",
                table: "Aufgaben",
                column: "SystemSchluessel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aufgaben");
        }
    }
}
