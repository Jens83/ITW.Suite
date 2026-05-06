using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITW.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAktivitaetsLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AktivitaetsLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Bereich = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Kategorie = table.Column<int>(type: "int", nullable: false),
                    IconCssClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Zeitpunkt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AktivitaetsLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllgemeineMitarbeiterprofile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Vorname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschaeftigungsart = table.Column<int>(type: "int", nullable: false),
                    Telefonnummer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Strasse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Hausnummer = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Postleitzahl = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllgemeineMitarbeiterprofile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutoplanLernereignisse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstplanPeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    BesetzungsSlotCode = table.Column<int>(type: "int", nullable: false),
                    EreignisTypCode = table.Column<int>(type: "int", nullable: false),
                    VorherigeUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NeueUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UrspruenglichGeplanterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    KontextArztUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    KontextNotfallsanitaeter1UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    KontextNotfallsanitaeter2UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AusfallGrundCode = table.Column<int>(type: "int", nullable: true),
                    BearbeitetVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ErfasstAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoplanLernereignisse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenutzerBereichszuordnungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Bereich = table.Column<int>(type: "int", nullable: false),
                    Rolle = table.Column<int>(type: "int", nullable: false),
                    Fuehrungsverantwortung = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ZugewiesenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeaktiviertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenutzerBereichszuordnungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DienstplanPerioden",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Jahr = table.Column<int>(type: "int", nullable: false),
                    Monat = table.Column<int>(type: "int", nullable: false),
                    Bezeichnung = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WunschphaseIstOffen = table.Column<bool>(type: "bit", nullable: false),
                    PlanIstFreigegeben = table.Column<bool>(type: "bit", nullable: false),
                    PlanFreigegebenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PlanFreigegebenVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DienstplanPerioden", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DienstplanWuensche",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstplanPeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WunschDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    WunschTyp = table.Column<int>(type: "int", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DienstplanWuensche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrtenbuchEintraege",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrzeugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FahrerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BeifahrerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RouteSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EinsatzId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FahrtKategorie = table.Column<int>(type: "int", nullable: false),
                    Fahrtzweck = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StartzeitUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndzeitUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Startort = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Zielort = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    StartKilometerstand = table.Column<int>(type: "int", nullable: false),
                    EndKilometerstand = table.Column<int>(type: "int", nullable: true),
                    GefahreneKilometer = table.Column<int>(type: "int", nullable: true),
                    TankmengeLiter = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    KilometerstandBeimTanken = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IstAutomatischVorbelegt = table.Column<bool>(type: "bit", nullable: false),
                    Bemerkung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AktualisiertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrtenbuchEintraege", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrzeugDokumente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrzeugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kategorie = table.Column<int>(type: "int", nullable: false),
                    Dateiname = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Bezeichnung = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Speicherpfad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "date", nullable: true),
                    HochgeladenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HochgeladenVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrzeugDokumente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fahrzeuge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InterneNummer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kennzeichen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Hersteller = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modell = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fahrzeugtyp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Baujahr = table.Column<int>(type: "int", nullable: true),
                    Erstzulassung = table.Column<DateOnly>(type: "date", nullable: true),
                    Kraftstoffart = table.Column<int>(type: "int", nullable: false),
                    LeistungKw = table.Column<int>(type: "int", nullable: true),
                    KilometerstandAktuell = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StandardStandort = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fahrzeuge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrzeugFahrerzuordnungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrzeugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ZuordnungTyp = table.Column<int>(type: "int", nullable: false),
                    IstPrimaer = table.Column<bool>(type: "bit", nullable: false),
                    GueltigVon = table.Column<DateOnly>(type: "date", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "date", nullable: true),
                    Bemerkung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrzeugFahrerzuordnungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrzeugPruefungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrzeugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    FaelligAm = table.Column<DateOnly>(type: "date", nullable: false),
                    LetzteErledigungAm = table.Column<DateOnly>(type: "date", nullable: true),
                    Bemerkung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrzeugPruefungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrzeugTrackingGeraete",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    LetzterKontaktAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrzeugTrackingGeraete", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FahrzeugVertraege",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FahrzeugId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VertragTyp = table.Column<int>(type: "int", nullable: false),
                    Anbieter = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Vertragsnummer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GueltigVon = table.Column<DateOnly>(type: "date", nullable: false),
                    GueltigBis = table.Column<DateOnly>(type: "date", nullable: true),
                    BetragProPeriode = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Periodizitaet = table.Column<int>(type: "int", nullable: true),
                    KuendigungsfristTage = table.Column<int>(type: "int", nullable: true),
                    DokumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notiz = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FahrzeugVertraege", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FreelancerMonatswuensche",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstplanPeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GewuenschteDienste = table.Column<int>(type: "int", nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelancerMonatswuensche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeplanteDiensttagAusfaelle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstplanPeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    BesetzungsSlotCode = table.Column<int>(type: "int", nullable: false),
                    UrspruenglichGeplanterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AusfallGrundCode = table.Column<int>(type: "int", nullable: false),
                    VertretungsUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ErfasstVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ErfasstAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeplanteDiensttagAusfaelle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeplanteDiensttage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstplanPeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DienstDatum = table.Column<DateOnly>(type: "date", nullable: false),
                    ArztUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notfallsanitaeter1UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notfallsanitaeter2UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeplanteDiensttage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItwMitarbeiterprofile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ErstelltAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AktualisiertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItwMitarbeiterprofile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItwQualifikationen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Bezeichnung = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sortierung = table.Column<int>(type: "int", nullable: false),
                    IsAktiv = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItwQualifikationen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MitarbeiterDokumente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Kategorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateinameOriginal = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    Speicherpfad = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Inhaltstyp = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateigroesseBytes = table.Column<long>(type: "bigint", nullable: false),
                    HochgeladenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HochgeladenVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MitarbeiterDokumente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulZuweisungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Modul = table.Column<int>(type: "int", nullable: false),
                    Bereich = table.Column<int>(type: "int", nullable: false),
                    Rolle = table.Column<int>(type: "int", nullable: false),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    ZugewiesenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ZugewiesenVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DeaktiviertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeaktiviertVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulZuweisungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswortResetAnfragen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Benutzername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Vorname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Bereich = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AngefordertAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    BearbeitetAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    BearbeitetVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswortResetAnfragen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackingGeraetEinrichtungscodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TabletName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    GueltigBisUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltAmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ErstelltVonUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EingeloestAmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingGeraetEinrichtungscodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackingGeraetStandorteAktuell",
                columns: table => new
                {
                    TrackingGeraetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    SpeedKmh = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    ErfasstAmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AktualisiertAmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingGeraetStandorteAktuell", x => x.TrackingGeraetId);
                });

            migrationBuilder.CreateTable(
                name: "TrackingGeraetStandortHistorie",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackingGeraetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    SpeedKmh = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    ErfasstAmUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingGeraetStandortHistorie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItwMitarbeiterQualifikationen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItwMitarbeiterprofilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QualifikationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IstHauptqualifikation = table.Column<bool>(type: "bit", nullable: false),
                    ZugewiesenAm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItwMitarbeiterQualifikationen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItwMitarbeiterQualifikationen_ItwMitarbeiterprofile_ItwMitarbeiterprofilId",
                        column: x => x.ItwMitarbeiterprofilId,
                        principalTable: "ItwMitarbeiterprofile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItwMitarbeiterQualifikationen_ItwQualifikationen_QualifikationId",
                        column: x => x.QualifikationId,
                        principalTable: "ItwQualifikationen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AktivitaetsLog_Bereich_Zeitpunkt",
                table: "AktivitaetsLog",
                columns: new[] { "Bereich", "Zeitpunkt" });

            migrationBuilder.CreateIndex(
                name: "IX_AllgemeineMitarbeiterprofile_UserId",
                table: "AllgemeineMitarbeiterprofile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AutoplanLernereignisse_ErfasstAm",
                table: "AutoplanLernereignisse",
                column: "ErfasstAm");

            migrationBuilder.CreateIndex(
                name: "IX_AutoplanLernereignisse_Periode_Datum",
                table: "AutoplanLernereignisse",
                columns: new[] { "DienstplanPeriodeId", "DienstDatum" });

            migrationBuilder.CreateIndex(
                name: "IX_BenutzerBereichszuordnungen_Bereich_Aktiv_Primaer",
                table: "BenutzerBereichszuordnungen",
                columns: new[] { "Bereich", "IsActive", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_BenutzerBereichszuordnungen_User_Aktiv_Primaer",
                table: "BenutzerBereichszuordnungen",
                columns: new[] { "UserId", "IsActive", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_DienstplanPerioden_Jahr_Monat",
                table: "DienstplanPerioden",
                columns: new[] { "Jahr", "Monat" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DienstplanPerioden_PlanIstFreigegeben",
                table: "DienstplanPerioden",
                column: "PlanIstFreigegeben");

            migrationBuilder.CreateIndex(
                name: "IX_DienstplanPerioden_WunschphaseIstOffen",
                table: "DienstplanPerioden",
                column: "WunschphaseIstOffen");

            migrationBuilder.CreateIndex(
                name: "IX_DienstplanWuensche_Periode_User_Datum",
                table: "DienstplanWuensche",
                columns: new[] { "DienstplanPeriodeId", "UserId", "WunschDatum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FahrtenbuchEintraege_EinsatzId",
                table: "FahrtenbuchEintraege",
                column: "EinsatzId");

            migrationBuilder.CreateIndex(
                name: "IX_FahrtenbuchEintraege_Fahrer_StartzeitUtc",
                table: "FahrtenbuchEintraege",
                columns: new[] { "FahrerUserId", "StartzeitUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FahrtenbuchEintraege_Fahrzeug_StartzeitUtc",
                table: "FahrtenbuchEintraege",
                columns: new[] { "FahrzeugId", "StartzeitUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FahrtenbuchEintraege_RouteSessionId",
                table: "FahrtenbuchEintraege",
                column: "RouteSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_FahrzeugDokumente_Fahrzeug_Kategorie",
                table: "FahrzeugDokumente",
                columns: new[] { "FahrzeugId", "Kategorie" });

            migrationBuilder.CreateIndex(
                name: "IX_Fahrzeuge_Status",
                table: "Fahrzeuge",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_Fahrzeuge_Kennzeichen",
                table: "Fahrzeuge",
                column: "Kennzeichen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Fahrzeuge_Vin",
                table: "Fahrzeuge",
                column: "Vin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FahrzeugFahrerzuordnungen_Fahrzeug_User_GueltigVon",
                table: "FahrzeugFahrerzuordnungen",
                columns: new[] { "FahrzeugId", "UserId", "GueltigVon" });

            migrationBuilder.CreateIndex(
                name: "IX_FahrzeugPruefungen_Fahrzeug_FaelligAm",
                table: "FahrzeugPruefungen",
                columns: new[] { "FahrzeugId", "FaelligAm" });

            migrationBuilder.CreateIndex(
                name: "UX_FahrzeugPruefungen_Fahrzeug_Typ",
                table: "FahrzeugPruefungen",
                columns: new[] { "FahrzeugId", "Typ" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_FahrzeugTrackingGeraete_DeviceIdentifier",
                table: "FahrzeugTrackingGeraete",
                column: "DeviceIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FahrzeugVertraege_Fahrzeug_VertragTyp_GueltigBis",
                table: "FahrzeugVertraege",
                columns: new[] { "FahrzeugId", "VertragTyp", "GueltigBis" });

            migrationBuilder.CreateIndex(
                name: "IX_FreelancerMonatswuensche_Periode_User",
                table: "FreelancerMonatswuensche",
                columns: new[] { "DienstplanPeriodeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeplanteDiensttagAusfaelle_Periode_Datum_Slot",
                table: "GeplanteDiensttagAusfaelle",
                columns: new[] { "DienstplanPeriodeId", "DienstDatum", "BesetzungsSlotCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeplanteDiensttage_Periode_Datum",
                table: "GeplanteDiensttage",
                columns: new[] { "DienstplanPeriodeId", "DienstDatum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItwMitarbeiterprofile_UserId",
                table: "ItwMitarbeiterprofile",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItwMitarbeiterQualifikationen_Profil_Qualifikation",
                table: "ItwMitarbeiterQualifikationen",
                columns: new[] { "ItwMitarbeiterprofilId", "QualifikationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItwMitarbeiterQualifikationen_QualifikationId",
                table: "ItwMitarbeiterQualifikationen",
                column: "QualifikationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItwQualifikationen_Code",
                table: "ItwQualifikationen",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MitarbeiterDokumente_UserId",
                table: "MitarbeiterDokumente",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MitarbeiterDokumente_UserId_HochgeladenAm",
                table: "MitarbeiterDokumente",
                columns: new[] { "UserId", "HochgeladenAm" });

            migrationBuilder.CreateIndex(
                name: "IX_ModulZuweisungen_Bereich_Rolle_Aktiv",
                table: "ModulZuweisungen",
                columns: new[] { "Bereich", "Rolle", "IstAktiv" });

            migrationBuilder.CreateIndex(
                name: "IX_ModulZuweisungen_Modul_Bereich_Rolle",
                table: "ModulZuweisungen",
                columns: new[] { "Modul", "Bereich", "Rolle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswortResetAnfragen_Bereich_Status",
                table: "PasswortResetAnfragen",
                columns: new[] { "Bereich", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswortResetAnfragen_User_Status",
                table: "PasswortResetAnfragen",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingGeraetEinrichtungscodes_CodeHash",
                table: "TrackingGeraetEinrichtungscodes",
                column: "CodeHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingGeraetEinrichtungscodes_Status_GueltigBis",
                table: "TrackingGeraetEinrichtungscodes",
                columns: new[] { "EingeloestAmUtc", "GueltigBisUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingGeraetStandorteAktuell_ErfasstAmUtc",
                table: "TrackingGeraetStandorteAktuell",
                column: "ErfasstAmUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingGeraetStandortHistorie_Geraet_ErfasstAmUtc",
                table: "TrackingGeraetStandortHistorie",
                columns: new[] { "TrackingGeraetId", "ErfasstAmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackingGeraetStandortHistorie_RouteSession_ErfasstAmUtc",
                table: "TrackingGeraetStandortHistorie",
                columns: new[] { "RouteSessionId", "ErfasstAmUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AktivitaetsLog");

            migrationBuilder.DropTable(
                name: "AllgemeineMitarbeiterprofile");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AutoplanLernereignisse");

            migrationBuilder.DropTable(
                name: "BenutzerBereichszuordnungen");

            migrationBuilder.DropTable(
                name: "DienstplanPerioden");

            migrationBuilder.DropTable(
                name: "DienstplanWuensche");

            migrationBuilder.DropTable(
                name: "FahrtenbuchEintraege");

            migrationBuilder.DropTable(
                name: "FahrzeugDokumente");

            migrationBuilder.DropTable(
                name: "Fahrzeuge");

            migrationBuilder.DropTable(
                name: "FahrzeugFahrerzuordnungen");

            migrationBuilder.DropTable(
                name: "FahrzeugPruefungen");

            migrationBuilder.DropTable(
                name: "FahrzeugTrackingGeraete");

            migrationBuilder.DropTable(
                name: "FahrzeugVertraege");

            migrationBuilder.DropTable(
                name: "FreelancerMonatswuensche");

            migrationBuilder.DropTable(
                name: "GeplanteDiensttagAusfaelle");

            migrationBuilder.DropTable(
                name: "GeplanteDiensttage");

            migrationBuilder.DropTable(
                name: "ItwMitarbeiterQualifikationen");

            migrationBuilder.DropTable(
                name: "MitarbeiterDokumente");

            migrationBuilder.DropTable(
                name: "ModulZuweisungen");

            migrationBuilder.DropTable(
                name: "PasswortResetAnfragen");

            migrationBuilder.DropTable(
                name: "TrackingGeraetEinrichtungscodes");

            migrationBuilder.DropTable(
                name: "TrackingGeraetStandorteAktuell");

            migrationBuilder.DropTable(
                name: "TrackingGeraetStandortHistorie");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ItwMitarbeiterprofile");

            migrationBuilder.DropTable(
                name: "ItwQualifikationen");
        }
    }
}
