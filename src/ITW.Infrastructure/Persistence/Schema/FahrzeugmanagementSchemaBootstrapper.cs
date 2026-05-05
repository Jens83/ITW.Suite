using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class FahrzeugmanagementSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugmanagementSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureFahrzeugmanagementSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[Fahrzeuge]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Fahrzeuge]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [InterneNummer] NVARCHAR(50) NOT NULL,
                [Kennzeichen] NVARCHAR(20) NOT NULL,
                [Vin] NVARCHAR(50) NOT NULL,
                [Hersteller] NVARCHAR(100) NOT NULL,
                [Modell] NVARCHAR(100) NOT NULL,
                [Fahrzeugtyp] NVARCHAR(100) NOT NULL,
                [Baujahr] INT NULL,
                [Erstzulassung] DATE NULL,
                [Kraftstoffart] INT NOT NULL,
                [LeistungKw] INT NULL,
                [KilometerstandAktuell] INT NOT NULL,
                [Status] INT NOT NULL,
                [StandardStandort] NVARCHAR(200) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                [AktualisiertAm] DATETIMEOFFSET(7) NOT NULL,
                [AktualisiertVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_Fahrzeuge] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'UX_Fahrzeuge_Kennzeichen'
              AND object_id = OBJECT_ID(N'[dbo].[Fahrzeuge]'))
        BEGIN
            CREATE UNIQUE INDEX [UX_Fahrzeuge_Kennzeichen]
                ON [dbo].[Fahrzeuge] ([Kennzeichen]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'UX_Fahrzeuge_Vin'
              AND object_id = OBJECT_ID(N'[dbo].[Fahrzeuge]'))
        BEGIN
            CREATE UNIQUE INDEX [UX_Fahrzeuge_Vin]
                ON [dbo].[Fahrzeuge] ([Vin]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_Fahrzeuge_Status'
              AND object_id = OBJECT_ID(N'[dbo].[Fahrzeuge]'))
        BEGIN
            CREATE INDEX [IX_Fahrzeuge_Status]
                ON [dbo].[Fahrzeuge] ([Status]);
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugDokumente]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrzeugDokumente]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [FahrzeugId] UNIQUEIDENTIFIER NOT NULL,
                [Kategorie] INT NOT NULL,
                [Dateiname] NVARCHAR(255) NOT NULL,
                [Bezeichnung] NVARCHAR(200) NOT NULL,
                [ContentType] NVARCHAR(150) NOT NULL,
                [Speicherpfad] NVARCHAR(500) NOT NULL,
                [GueltigBis] DATE NULL,
                [HochgeladenAm] DATETIMEOFFSET(7) NOT NULL,
                [HochgeladenVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_FahrzeugDokumente] PRIMARY KEY ([Id])
            );
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugDokumente]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrzeugDokumente', N'FahrzeugId') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrzeugDokumente]
            ADD [FahrzeugId] UNIQUEIDENTIFIER NOT NULL
                CONSTRAINT [DF_FahrzeugDokumente_FahrzeugId]
                DEFAULT '00000000-0000-0000-0000-000000000000';
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugDokumente]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrzeugDokumente', N'Bezeichnung') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrzeugDokumente]
            ADD [Bezeichnung] NVARCHAR(200) NOT NULL
                CONSTRAINT [DF_FahrzeugDokumente_Bezeichnung]
                DEFAULT N'Dokument';
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrzeugDokumente_Fahrzeug_Kategorie'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugDokumente]'))
        BEGIN
            CREATE INDEX [IX_FahrzeugDokumente_Fahrzeug_Kategorie]
                ON [dbo].[FahrzeugDokumente] ([FahrzeugId], [Kategorie]);
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugVertraege]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrzeugVertraege]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [FahrzeugId] UNIQUEIDENTIFIER NOT NULL,
                [VertragTyp] INT NOT NULL,
                [Anbieter] NVARCHAR(200) NOT NULL,
                [Vertragsnummer] NVARCHAR(100) NOT NULL,
                [GueltigVon] DATE NOT NULL,
                [GueltigBis] DATE NULL,
                [BetragProPeriode] DECIMAL(12,2) NULL,
                [Periodizitaet] INT NULL,
                [KuendigungsfristTage] INT NULL,
                [DokumentId] UNIQUEIDENTIFIER NULL,
                [Notiz] NVARCHAR(1000) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_FahrzeugVertraege] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrzeugVertraege_Fahrzeug_VertragTyp_GueltigBis'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugVertraege]'))
        BEGIN
            CREATE INDEX [IX_FahrzeugVertraege_Fahrzeug_VertragTyp_GueltigBis]
                ON [dbo].[FahrzeugVertraege] ([FahrzeugId], [VertragTyp], [GueltigBis]);
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugFahrerzuordnungen]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrzeugFahrerzuordnungen]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [FahrzeugId] UNIQUEIDENTIFIER NOT NULL,
                [UserId] NVARCHAR(450) NOT NULL,
                [ZuordnungTyp] INT NOT NULL,
                [IstPrimaer] BIT NOT NULL,
                [GueltigVon] DATE NOT NULL,
                [GueltigBis] DATE NULL,
                [Bemerkung] NVARCHAR(500) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_FahrzeugFahrerzuordnungen] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrzeugFahrerzuordnungen_Fahrzeug_User_GueltigVon'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugFahrerzuordnungen]'))
        BEGIN
            CREATE INDEX [IX_FahrzeugFahrerzuordnungen_Fahrzeug_User_GueltigVon]
                ON [dbo].[FahrzeugFahrerzuordnungen] ([FahrzeugId], [UserId], [GueltigVon]);
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugTrackingGeraete]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrzeugTrackingGeraete]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DeviceIdentifier] NVARCHAR(150) NOT NULL,
                [ApiKeyHash] NVARCHAR(500) NOT NULL,
                [IstAktiv] BIT NOT NULL,
                [LetzterKontaktAm] DATETIMEOFFSET(7) NULL,
                CONSTRAINT [PK_FahrzeugTrackingGeraete] PRIMARY KEY ([Id])
            );
        END;

        IF OBJECT_ID(N'[dbo].[FahrzeugTrackingGeraete]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrzeugTrackingGeraete', N'FahrzeugId') IS NOT NULL
           AND EXISTS (
                SELECT 1
                FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[FahrzeugTrackingGeraete]')
                  AND name = N'FahrzeugId'
                  AND is_nullable = 0
           )
        BEGIN
            ALTER TABLE [dbo].[FahrzeugTrackingGeraete]
            ALTER COLUMN [FahrzeugId] UNIQUEIDENTIFIER NULL;
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'UX_FahrzeugTrackingGeraete_DeviceIdentifier'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugTrackingGeraete]'))
        BEGIN
            CREATE UNIQUE INDEX [UX_FahrzeugTrackingGeraete_DeviceIdentifier]
                ON [dbo].[FahrzeugTrackingGeraete] ([DeviceIdentifier]);
        END;

                IF OBJECT_ID(N'[dbo].[TrackingGeraetEinrichtungscodes]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[TrackingGeraetEinrichtungscodes]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [TabletName] NVARCHAR(120) NOT NULL,
                [DeviceIdentifier] NVARCHAR(150) NOT NULL,
                [CodeHash] NVARCHAR(128) NOT NULL,
                [GueltigBisUtc] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltAmUtc] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NULL,
                [EingeloestAmUtc] DATETIMEOFFSET(7) NULL,
                CONSTRAINT [PK_TrackingGeraetEinrichtungscodes] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_TrackingGeraetEinrichtungscodes_CodeHash'
              AND object_id = OBJECT_ID(N'[dbo].[TrackingGeraetEinrichtungscodes]'))
        BEGIN
            CREATE INDEX [IX_TrackingGeraetEinrichtungscodes_CodeHash]
                ON [dbo].[TrackingGeraetEinrichtungscodes] ([CodeHash]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_TrackingGeraetEinrichtungscodes_Status_GueltigBis'
              AND object_id = OBJECT_ID(N'[dbo].[TrackingGeraetEinrichtungscodes]'))
        BEGIN
            CREATE INDEX [IX_TrackingGeraetEinrichtungscodes_Status_GueltigBis]
                ON [dbo].[TrackingGeraetEinrichtungscodes] ([EingeloestAmUtc], [GueltigBisUtc]);
        END;

        IF OBJECT_ID(N'[dbo].[TrackingGeraetStandorteAktuell]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[TrackingGeraetStandorteAktuell]
            (
                [TrackingGeraetId] UNIQUEIDENTIFIER NOT NULL,
                [RouteSessionId] UNIQUEIDENTIFIER NOT NULL,
                [Latitude] DECIMAL(9,6) NOT NULL,
                [Longitude] DECIMAL(9,6) NOT NULL,
                [SpeedKmh] DECIMAL(6,2) NOT NULL,
                [ErfasstAmUtc] DATETIMEOFFSET(7) NOT NULL,
                [DeviceIdentifier] NVARCHAR(150) NOT NULL,
                [AktualisiertAmUtc] DATETIMEOFFSET(7) NOT NULL,
                CONSTRAINT [PK_TrackingGeraetStandorteAktuell] PRIMARY KEY ([TrackingGeraetId])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_TrackingGeraetStandorteAktuell_ErfasstAmUtc'
              AND object_id = OBJECT_ID(N'[dbo].[TrackingGeraetStandorteAktuell]'))
        BEGIN
            CREATE INDEX [IX_TrackingGeraetStandorteAktuell_ErfasstAmUtc]
                ON [dbo].[TrackingGeraetStandorteAktuell] ([ErfasstAmUtc]);
        END;

        IF OBJECT_ID(N'[dbo].[TrackingGeraetStandortHistorie]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[TrackingGeraetStandortHistorie]
            (
                [Id] BIGINT IDENTITY(1,1) NOT NULL,
                [TrackingGeraetId] UNIQUEIDENTIFIER NOT NULL,
                [RouteSessionId] UNIQUEIDENTIFIER NOT NULL,
                [Latitude] DECIMAL(9,6) NOT NULL,
                [Longitude] DECIMAL(9,6) NOT NULL,
                [SpeedKmh] DECIMAL(6,2) NOT NULL,
                [ErfasstAmUtc] DATETIMEOFFSET(7) NOT NULL,
                [DeviceIdentifier] NVARCHAR(150) NOT NULL,
                CONSTRAINT [PK_TrackingGeraetStandortHistorie] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_TrackingGeraetStandortHistorie_Geraet_ErfasstAmUtc'
              AND object_id = OBJECT_ID(N'[dbo].[TrackingGeraetStandortHistorie]'))
        BEGIN
            CREATE INDEX [IX_TrackingGeraetStandortHistorie_Geraet_ErfasstAmUtc]
                ON [dbo].[TrackingGeraetStandortHistorie] ([TrackingGeraetId], [ErfasstAmUtc]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_TrackingGeraetStandortHistorie_RouteSession_ErfasstAmUtc'
              AND object_id = OBJECT_ID(N'[dbo].[TrackingGeraetStandortHistorie]'))
        BEGIN
            CREATE INDEX [IX_TrackingGeraetStandortHistorie_RouteSession_ErfasstAmUtc]
                ON [dbo].[TrackingGeraetStandortHistorie] ([RouteSessionId], [ErfasstAmUtc]);
        END;

        IF OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrtenbuchEintraege]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [FahrzeugId] UNIQUEIDENTIFIER NOT NULL,
                [FahrerUserId] NVARCHAR(450) NOT NULL,
                [FahrerName] NVARCHAR(200) NOT NULL,
                [BeifahrerName] NVARCHAR(200) NULL,
                [RouteSessionId] UNIQUEIDENTIFIER NULL,
                [EinsatzId] UNIQUEIDENTIFIER NULL,
                [FahrtKategorie] INT NOT NULL,
                [Fahrtzweck] NVARCHAR(300) NOT NULL,
                [StartzeitUtc] DATETIMEOFFSET(7) NOT NULL,
                [EndzeitUtc] DATETIMEOFFSET(7) NULL,
                [Startort] NVARCHAR(300) NULL,
                [Zielort] NVARCHAR(300) NULL,
                [StartKilometerstand] INT NOT NULL,
                [EndKilometerstand] INT NULL,
                [GefahreneKilometer] INT NULL,
                [TankmengeLiter] DECIMAL(10,2) NULL,
                [KilometerstandBeimTanken] INT NULL,
                [Status] INT NOT NULL,
                [IstAutomatischVorbelegt] BIT NOT NULL,
                [Bemerkung] NVARCHAR(1000) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                [AktualisiertAm] DATETIMEOFFSET(7) NULL,
                [AktualisiertVonUserId] NVARCHAR(450) NULL,
                CONSTRAINT [PK_FahrtenbuchEintraege] PRIMARY KEY ([Id])
            );
        END;

                IF OBJECT_ID(N'[dbo].[FahrzeugPruefungen]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FahrzeugPruefungen]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [FahrzeugId] UNIQUEIDENTIFIER NOT NULL,
                [Typ] INT NOT NULL,
                [FaelligAm] DATE NOT NULL,
                [LetzteErledigungAm] DATE NULL,
                [Bemerkung] NVARCHAR(1000) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                [AktualisiertAm] DATETIMEOFFSET(7) NOT NULL,
                [AktualisiertVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_FahrzeugPruefungen] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'UX_FahrzeugPruefungen_Fahrzeug_Typ'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugPruefungen]'))
        BEGIN
            CREATE UNIQUE INDEX [UX_FahrzeugPruefungen_Fahrzeug_Typ]
                ON [dbo].[FahrzeugPruefungen] ([FahrzeugId], [Typ]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrzeugPruefungen_Fahrzeug_FaelligAm'
              AND object_id = OBJECT_ID(N'[dbo].[FahrzeugPruefungen]'))
        BEGIN
            CREATE INDEX [IX_FahrzeugPruefungen_Fahrzeug_FaelligAm]
                ON [dbo].[FahrzeugPruefungen] ([FahrzeugId], [FaelligAm]);
        END;

                IF OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrtenbuchEintraege', N'FahrerName') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrtenbuchEintraege]
            ADD [FahrerName] NVARCHAR(200) NOT NULL
                CONSTRAINT [DF_FahrtenbuchEintraege_FahrerName]
                DEFAULT N'Unbekannt';
        END;

        IF OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrtenbuchEintraege', N'BeifahrerName') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrtenbuchEintraege]
            ADD [BeifahrerName] NVARCHAR(200) NULL;
        END;

        IF OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrtenbuchEintraege', N'TankmengeLiter') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrtenbuchEintraege]
            ADD [TankmengeLiter] DECIMAL(10,2) NULL;
        END;

        IF OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.FahrtenbuchEintraege', N'KilometerstandBeimTanken') IS NULL
        BEGIN
            ALTER TABLE [dbo].[FahrtenbuchEintraege]
            ADD [KilometerstandBeimTanken] INT NULL;
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrtenbuchEintraege_Fahrzeug_StartzeitUtc'
              AND object_id = OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]'))
        BEGIN
            CREATE INDEX [IX_FahrtenbuchEintraege_Fahrzeug_StartzeitUtc]
                ON [dbo].[FahrtenbuchEintraege] ([FahrzeugId], [StartzeitUtc]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrtenbuchEintraege_Fahrer_StartzeitUtc'
              AND object_id = OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]'))
        BEGIN
            CREATE INDEX [IX_FahrtenbuchEintraege_Fahrer_StartzeitUtc]
                ON [dbo].[FahrtenbuchEintraege] ([FahrerUserId], [StartzeitUtc]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrtenbuchEintraege_RouteSessionId'
              AND object_id = OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]'))
        BEGIN
            CREATE INDEX [IX_FahrtenbuchEintraege_RouteSessionId]
                ON [dbo].[FahrtenbuchEintraege] ([RouteSessionId]);
        END;

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_FahrtenbuchEintraege_EinsatzId'
              AND object_id = OBJECT_ID(N'[dbo].[FahrtenbuchEintraege]'))
        BEGIN
            CREATE INDEX [IX_FahrtenbuchEintraege_EinsatzId]
                ON [dbo].[FahrtenbuchEintraege] ([EinsatzId]);
        END;
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}