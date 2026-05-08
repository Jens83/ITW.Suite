using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITW.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLagermanagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Lager");

            migrationBuilder.CreateTable(
                name: "Artikel",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kategorie = table.Column<int>(type: "int", nullable: false),
                    BasisEinheit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PackungsGroesse = table.Column<int>(type: "int", nullable: true),
                    PackungsEinheit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VerbrauchProPatient = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    HatAblaufdatum = table.Column<bool>(type: "bit", nullable: false),
                    Mindestbestand = table.Column<int>(type: "int", nullable: false),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artikel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtikelBestand",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtikelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lagerort = table.Column<int>(type: "int", nullable: false),
                    Menge = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtikelBestand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtikelCharge",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtikelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lagerort = table.Column<int>(type: "int", nullable: false),
                    Menge = table.Column<int>(type: "int", nullable: false),
                    Ablaufdatum = table.Column<DateOnly>(type: "date", nullable: false),
                    ChargeNummer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    EingebuchtAm = table.Column<DateOnly>(type: "date", nullable: false),
                    EingebuchtVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IstAusgebucht = table.Column<bool>(type: "bit", nullable: false),
                    AusgebuchtAm = table.Column<DateOnly>(type: "date", nullable: true),
                    AusgebuchtVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtikelCharge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EinsatzVerbrauch",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Datum = table.Column<DateOnly>(type: "date", nullable: false),
                    Fahrzeug = table.Column<int>(type: "int", nullable: false),
                    Patienten = table.Column<int>(type: "int", nullable: false),
                    Bemerkung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinsatzVerbrauch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SauerstoffBewegung",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlascheId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    VonLagerort = table.Column<int>(type: "int", nullable: true),
                    NachLagerort = table.Column<int>(type: "int", nullable: true),
                    Datum = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Bemerkung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SauerstoffBewegung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SauerstoffFlasche",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bezeichnung = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Lagerort = table.Column<int>(type: "int", nullable: false),
                    VollEingebuchtAmDepot = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SauerstoffFlasche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EinsatzVerbrauchPosition",
                schema: "Lager",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EinsatzVerbrauchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArtikelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Menge = table.Column<int>(type: "int", nullable: false),
                    IstProPatientBerechnet = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EinsatzVerbrauchPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EinsatzVerbrauchPosition_EinsatzVerbrauch_EinsatzVerbrauchId",
                        column: x => x.EinsatzVerbrauchId,
                        principalSchema: "Lager",
                        principalTable: "EinsatzVerbrauch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Lager_Artikel_Name",
                schema: "Lager",
                table: "Artikel",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Lager_ArtikelBestand_Artikel_Lagerort",
                schema: "Lager",
                table: "ArtikelBestand",
                columns: new[] { "ArtikelId", "Lagerort" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lager_ArtikelCharge_Artikel_Ablauf",
                schema: "Lager",
                table: "ArtikelCharge",
                columns: new[] { "ArtikelId", "Ablaufdatum" },
                filter: "[IstAusgebucht] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Lager_EinsatzVerbrauchPosition_Verbrauch",
                schema: "Lager",
                table: "EinsatzVerbrauchPosition",
                column: "EinsatzVerbrauchId");

            migrationBuilder.CreateIndex(
                name: "IX_Lager_SauerstoffBewegung_Flasche_Datum",
                schema: "Lager",
                table: "SauerstoffBewegung",
                columns: new[] { "FlascheId", "Datum" });

            migrationBuilder.CreateIndex(
                name: "IX_Lager_SauerstoffFlasche_Status_Lagerort",
                schema: "Lager",
                table: "SauerstoffFlasche",
                columns: new[] { "Status", "Lagerort" },
                filter: "[IstAktiv] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artikel",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "ArtikelBestand",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "ArtikelCharge",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "EinsatzVerbrauchPosition",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "SauerstoffBewegung",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "SauerstoffFlasche",
                schema: "Lager");

            migrationBuilder.DropTable(
                name: "EinsatzVerbrauch",
                schema: "Lager");
        }
    }
}
