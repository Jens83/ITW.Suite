using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITW.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSauerstoffLieferungAndRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lager_SauerstoffFlasche_Status_Lagerort",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "Bezeichnung",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "Lagerort",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "VollEingebuchtAmDepot",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "NachLagerort",
                schema: "Lager",
                table: "SauerstoffBewegung");

            migrationBuilder.DropColumn(
                name: "VonLagerort",
                schema: "Lager",
                table: "SauerstoffBewegung");

            migrationBuilder.AddColumn<Guid>(
                name: "FahrzeugId",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LieferungId",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "NachFahrzeugId",
                schema: "Lager",
                table: "SauerstoffBewegung",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VonFahrzeugId",
                schema: "Lager",
                table: "SauerstoffBewegung",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SauerstoffLieferung",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LieferscheinNummer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lieferdatum = table.Column<DateOnly>(type: "date", nullable: false),
                    Bemerkung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErfasstAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErfasstVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SauerstoffLieferung", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lager_SauerstoffFlasche_Status_Fahrzeug",
                schema: "Lager",
                table: "SauerstoffFlasche",
                columns: new[] { "Status", "FahrzeugId" },
                filter: "[IstAktiv] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SauerstoffFlasche_LieferungId",
                schema: "Lager",
                table: "SauerstoffFlasche",
                column: "LieferungId");

            migrationBuilder.CreateIndex(
                name: "UX_Lager_SauerstoffLieferung_Schein",
                schema: "Lager",
                table: "SauerstoffLieferung",
                column: "LieferscheinNummer",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lager_SauerstoffFlasche_Lieferung",
                schema: "Lager",
                table: "SauerstoffFlasche",
                column: "LieferungId",
                principalSchema: "Lager",
                principalTable: "SauerstoffLieferung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lager_SauerstoffFlasche_Lieferung",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropTable(
                name: "SauerstoffLieferung",
                schema: "Lager");

            migrationBuilder.DropIndex(
                name: "IX_Lager_SauerstoffFlasche_Status_Fahrzeug",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropIndex(
                name: "IX_SauerstoffFlasche_LieferungId",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "FahrzeugId",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "LieferungId",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "NachFahrzeugId",
                schema: "Lager",
                table: "SauerstoffBewegung");

            migrationBuilder.DropColumn(
                name: "VonFahrzeugId",
                schema: "Lager",
                table: "SauerstoffBewegung");

            migrationBuilder.AddColumn<string>(
                name: "Bezeichnung",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Lagerort",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VollEingebuchtAmDepot",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NachLagerort",
                schema: "Lager",
                table: "SauerstoffBewegung",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VonLagerort",
                schema: "Lager",
                table: "SauerstoffBewegung",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lager_SauerstoffFlasche_Status_Lagerort",
                schema: "Lager",
                table: "SauerstoffFlasche",
                columns: new[] { "Status", "Lagerort" },
                filter: "[IstAktiv] = 1");
        }
    }
}
