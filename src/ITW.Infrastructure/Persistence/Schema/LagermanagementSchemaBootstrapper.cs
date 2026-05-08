using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class LagermanagementSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public LagermanagementSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureLagermanagementSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Lager')
                EXEC('CREATE SCHEMA [Lager]');

            -- ═══════════════════════════════════════════════════════════════
            -- Artikel
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[Artikel]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[Artikel]
                (
                    [Id]                    UNIQUEIDENTIFIER   NOT NULL,
                    [Name]                  NVARCHAR(200)      NOT NULL,
                    [Kategorie]             INT                NOT NULL,
                    [BasisEinheit]          NVARCHAR(50)       NOT NULL,
                    [PackungsGroesse]       INT                NULL,
                    [PackungsEinheit]       NVARCHAR(50)       NULL,
                    [VerbrauchProPatient]   DECIMAL(10,4)      NULL,
                    [HatAblaufdatum]        BIT                NOT NULL DEFAULT 0,
                    [Mindestbestand]        INT                NOT NULL DEFAULT 0,
                    [IstAktiv]              BIT                NOT NULL DEFAULT 1,
                    [ErstelltAm]            DATETIMEOFFSET(7)  NOT NULL,
                    [ErstelltVonUserId]     NVARCHAR(450)      NOT NULL,
                    [AktualisiertAm]        DATETIMEOFFSET(7)  NOT NULL,
                    [AktualisiertVonUserId] NVARCHAR(450)      NOT NULL,
                    CONSTRAINT [PK_Lager_Artikel] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [UX_Lager_Artikel_Name] ON [Lager].[Artikel] ([Name]);
                CREATE INDEX [IX_Lager_Artikel_Kategorie_IstAktiv] ON [Lager].[Artikel] ([Kategorie], [IstAktiv]);
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- ArtikelBestand
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[ArtikelBestand]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[ArtikelBestand]
                (
                    [Id]        UNIQUEIDENTIFIER NOT NULL,
                    [ArtikelId] UNIQUEIDENTIFIER NOT NULL,
                    [Lagerort]  INT              NOT NULL,
                    [Menge]     INT              NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_Lager_ArtikelBestand] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Lager_ArtikelBestand_Artikel]
                        FOREIGN KEY ([ArtikelId]) REFERENCES [Lager].[Artikel]([Id])
                );
                CREATE UNIQUE INDEX [UX_Lager_ArtikelBestand_Artikel_Lagerort]
                    ON [Lager].[ArtikelBestand] ([ArtikelId], [Lagerort]);
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- ArtikelCharge
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[ArtikelCharge]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[ArtikelCharge]
                (
                    [Id]                   UNIQUEIDENTIFIER NOT NULL,
                    [ArtikelId]            UNIQUEIDENTIFIER NOT NULL,
                    [Lagerort]             INT              NOT NULL,
                    [Menge]                INT              NOT NULL,
                    [Ablaufdatum]          DATE             NOT NULL,
                    [ChargeNummer]         NVARCHAR(100)    NOT NULL DEFAULT '',
                    [EingebuchtAm]         DATE             NOT NULL,
                    [EingebuchtVonUserId]  NVARCHAR(450)    NOT NULL,
                    [IstAusgebucht]        BIT              NOT NULL DEFAULT 0,
                    [AusgebuchtAm]         DATE             NULL,
                    [AusgebuchtVonUserId]  NVARCHAR(450)    NULL,
                    CONSTRAINT [PK_Lager_ArtikelCharge] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Lager_ArtikelCharge_Artikel]
                        FOREIGN KEY ([ArtikelId]) REFERENCES [Lager].[Artikel]([Id])
                );
                CREATE INDEX [IX_Lager_ArtikelCharge_Artikel_Ablauf]
                    ON [Lager].[ArtikelCharge] ([ArtikelId], [Ablaufdatum]) WHERE [IstAusgebucht] = 0;
                CREATE INDEX [IX_Lager_ArtikelCharge_Ablaufdatum]
                    ON [Lager].[ArtikelCharge] ([Ablaufdatum]) WHERE [IstAusgebucht] = 0;
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- EinsatzVerbrauch
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[EinsatzVerbrauch]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[EinsatzVerbrauch]
                (
                    [Id]                UNIQUEIDENTIFIER  NOT NULL,
                    [Datum]             DATE              NOT NULL,
                    [Fahrzeug]          INT               NOT NULL,
                    [Patienten]         INT               NOT NULL DEFAULT 0,
                    [Bemerkung]         NVARCHAR(500)     NULL,
                    [ErstelltAm]        DATETIMEOFFSET(7) NOT NULL,
                    [ErstelltVonUserId] NVARCHAR(450)     NOT NULL,
                    CONSTRAINT [PK_Lager_EinsatzVerbrauch] PRIMARY KEY ([Id])
                );
                CREATE INDEX [IX_Lager_EinsatzVerbrauch_Datum]
                    ON [Lager].[EinsatzVerbrauch] ([Datum] DESC);
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- EinsatzVerbrauchPosition
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[EinsatzVerbrauchPosition]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[EinsatzVerbrauchPosition]
                (
                    [Id]                     UNIQUEIDENTIFIER NOT NULL,
                    [EinsatzVerbrauchId]     UNIQUEIDENTIFIER NOT NULL,
                    [ArtikelId]              UNIQUEIDENTIFIER NOT NULL,
                    [Menge]                  INT              NOT NULL,
                    [IstProPatientBerechnet] BIT              NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_Lager_EinsatzVerbrauchPosition] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Lager_EinsatzVerbrauchPosition_Verbrauch]
                        FOREIGN KEY ([EinsatzVerbrauchId]) REFERENCES [Lager].[EinsatzVerbrauch]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_Lager_EinsatzVerbrauchPosition_Artikel]
                        FOREIGN KEY ([ArtikelId]) REFERENCES [Lager].[Artikel]([Id])
                );
                CREATE INDEX [IX_Lager_EinsatzVerbrauchPosition_Verbrauch]
                    ON [Lager].[EinsatzVerbrauchPosition] ([EinsatzVerbrauchId]);
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- SauerstoffLieferung (NEU)
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[SauerstoffLieferung]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[SauerstoffLieferung]
                (
                    [Id]                 UNIQUEIDENTIFIER  NOT NULL,
                    [LieferscheinNummer] NVARCHAR(100)     NOT NULL,
                    [Lieferdatum]        DATE              NOT NULL,
                    [Bemerkung]          NVARCHAR(500)     NULL,
                    [ErfasstAm]          DATETIMEOFFSET(7) NOT NULL,
                    [ErfasstVonUserId]   NVARCHAR(450)     NOT NULL,
                    CONSTRAINT [PK_Lager_SauerstoffLieferung] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [UX_Lager_SauerstoffLieferung_Schein]
                    ON [Lager].[SauerstoffLieferung] ([LieferscheinNummer]);
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- SauerstoffFlasche
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[SauerstoffFlasche]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[SauerstoffFlasche]
                (
                    [Id]                UNIQUEIDENTIFIER  NOT NULL,
                    [LieferungId]       UNIQUEIDENTIFIER  NOT NULL,
                    [Groesse]           INT               NOT NULL DEFAULT 10,
                    [FlaschenNummer]    NVARCHAR(100)     NULL,
                    [Status]            INT               NOT NULL DEFAULT 0,
                    [FahrzeugId]        UNIQUEIDENTIFIER  NULL,
                    [IstAktiv]          BIT               NOT NULL DEFAULT 1,
                    [ErstelltAm]        DATETIMEOFFSET(7) NOT NULL,
                    [ErstelltVonUserId] NVARCHAR(450)     NOT NULL,
                    CONSTRAINT [PK_Lager_SauerstoffFlasche] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Lager_SauerstoffFlasche_Lieferung]
                        FOREIGN KEY ([LieferungId]) REFERENCES [Lager].[SauerstoffLieferung]([Id])
                );
                EXEC('CREATE UNIQUE INDEX [UX_Lager_SauerstoffFlasche_FlaschenNummer]
                          ON [Lager].[SauerstoffFlasche] ([FlaschenNummer])
                          WHERE [FlaschenNummer] IS NOT NULL');
                CREATE INDEX [IX_Lager_SauerstoffFlasche_Status_Fahrzeug]
                    ON [Lager].[SauerstoffFlasche] ([Status], [FahrzeugId]) WHERE [IstAktiv] = 1;
            END
            ELSE
            BEGIN
                -- Migriere altes Schema (Lagerort-Spalte vorhanden) auf neues Schema
                IF EXISTS (SELECT 1 FROM sys.columns
                           WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'Lagerort')
                BEGIN
                    DECLARE @MigLieferungId UNIQUEIDENTIFIER = NEWID();

                    -- Platzhalter-Lieferung für bestehende Flaschen anlegen
                    IF NOT EXISTS (SELECT 1 FROM [Lager].[SauerstoffLieferung] WHERE [LieferscheinNummer] = N'MIGRATION')
                        INSERT INTO [Lager].[SauerstoffLieferung]
                               ([Id], [LieferscheinNummer], [Lieferdatum], [Bemerkung], [ErfasstAm], [ErfasstVonUserId])
                        VALUES (@MigLieferungId, N'MIGRATION', CAST(GETDATE() AS DATE),
                                N'Automatisch migriert', GETUTCDATE(), N'SYSTEM');
                    ELSE
                        SELECT @MigLieferungId = [Id] FROM [Lager].[SauerstoffLieferung]
                        WHERE [LieferscheinNummer] = N'MIGRATION';

                    -- FK zu SauerstoffBewegung entfernen falls vorhanden
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Lager_SauerstoffBewegung_Flasche')
                        EXEC('ALTER TABLE [Lager].[SauerstoffBewegung] DROP CONSTRAINT [FK_Lager_SauerstoffBewegung_Flasche]');

                    -- Neue Spalten hinzufügen
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'LieferungId')
                        ALTER TABLE [Lager].[SauerstoffFlasche] ADD [LieferungId] UNIQUEIDENTIFIER NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'FahrzeugId')
                        ALTER TABLE [Lager].[SauerstoffFlasche] ADD [FahrzeugId] UNIQUEIDENTIFIER NULL;

                    -- Bestehende Zeilen auf Migrations-Lieferung setzen (EXEC wegen Compile-Zeit-Validierung)
                    DECLARE @IdStr NVARCHAR(36) = CAST(@MigLieferungId AS NVARCHAR(36));
                    EXEC('UPDATE [Lager].[SauerstoffFlasche] SET [LieferungId] = ''' + @IdStr + ''' WHERE [LieferungId] IS NULL');

                    -- LieferungId NOT NULL machen
                    EXEC('ALTER TABLE [Lager].[SauerstoffFlasche] ALTER COLUMN [LieferungId] UNIQUEIDENTIFIER NOT NULL');

                    -- Alle alten Indizes entfernen die Lagerort referenzieren
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'IX_Lager_SauerstoffFlasche_Status_Lagerort')
                        DROP INDEX [IX_Lager_SauerstoffFlasche_Status_Lagerort] ON [Lager].[SauerstoffFlasche];

                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'IX_Lager_SauerstoffFlasche_VollImDepot')
                        DROP INDEX [IX_Lager_SauerstoffFlasche_VollImDepot] ON [Lager].[SauerstoffFlasche];

                    -- Bezeichnung: ggf. DEFAULT-Constraint entfernen
                    DECLARE @DefName NVARCHAR(200);
                    SELECT @DefName = dc.name FROM sys.default_constraints dc
                        JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                        WHERE c.object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND c.name = N'Bezeichnung';
                    IF @DefName IS NOT NULL
                        EXEC('ALTER TABLE [Lager].[SauerstoffFlasche] DROP CONSTRAINT [' + @DefName + ']');

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'Bezeichnung')
                        ALTER TABLE [Lager].[SauerstoffFlasche] DROP COLUMN [Bezeichnung];

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'Lagerort')
                        ALTER TABLE [Lager].[SauerstoffFlasche] DROP COLUMN [Lagerort];

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'VollEingebuchtAmDepot')
                        ALTER TABLE [Lager].[SauerstoffFlasche] DROP COLUMN [VollEingebuchtAmDepot];

                    -- FK zu Lieferung hinzufügen
                    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Lager_SauerstoffFlasche_Lieferung')
                        EXEC('ALTER TABLE [Lager].[SauerstoffFlasche]
                                  ADD CONSTRAINT [FK_Lager_SauerstoffFlasche_Lieferung]
                                      FOREIGN KEY ([LieferungId]) REFERENCES [Lager].[SauerstoffLieferung]([Id])');

                    -- Index für FlaschenNummer
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'UX_Lager_SauerstoffFlasche_FlaschenNummer')
                        EXEC('CREATE UNIQUE INDEX [UX_Lager_SauerstoffFlasche_FlaschenNummer]
                                  ON [Lager].[SauerstoffFlasche] ([FlaschenNummer])
                                  WHERE [FlaschenNummer] IS NOT NULL');

                    -- Neuer Status/FahrzeugId-Index
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffFlasche]') AND name = N'IX_Lager_SauerstoffFlasche_Status_Fahrzeug')
                        EXEC('CREATE INDEX [IX_Lager_SauerstoffFlasche_Status_Fahrzeug]
                                  ON [Lager].[SauerstoffFlasche] ([Status], [FahrzeugId]) WHERE [IstAktiv] = 1');
                END;
            END;

            -- ═══════════════════════════════════════════════════════════════
            -- SauerstoffBewegung
            -- ═══════════════════════════════════════════════════════════════
            IF OBJECT_ID(N'[Lager].[SauerstoffBewegung]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Lager].[SauerstoffBewegung]
                (
                    [Id]                UNIQUEIDENTIFIER  NOT NULL,
                    [FlascheId]         UNIQUEIDENTIFIER  NOT NULL,
                    [Typ]               INT               NOT NULL,
                    [VonFahrzeugId]     UNIQUEIDENTIFIER  NULL,
                    [NachFahrzeugId]    UNIQUEIDENTIFIER  NULL,
                    [Datum]             DATETIMEOFFSET(7) NOT NULL,
                    [ErstelltVonUserId] NVARCHAR(450)     NOT NULL,
                    [Bemerkung]         NVARCHAR(500)     NULL,
                    CONSTRAINT [PK_Lager_SauerstoffBewegung] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Lager_SauerstoffBewegung_Flasche]
                        FOREIGN KEY ([FlascheId]) REFERENCES [Lager].[SauerstoffFlasche]([Id])
                );
                CREATE INDEX [IX_Lager_SauerstoffBewegung_Flasche_Datum]
                    ON [Lager].[SauerstoffBewegung] ([FlascheId], [Datum] DESC);
            END
            ELSE
            BEGIN
                -- Migriere altes Schema (VonLagerort-Spalte) auf neues
                IF EXISTS (SELECT 1 FROM sys.columns
                           WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffBewegung]') AND name = N'VonLagerort')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffBewegung]') AND name = N'VonFahrzeugId')
                        ALTER TABLE [Lager].[SauerstoffBewegung] ADD [VonFahrzeugId] UNIQUEIDENTIFIER NULL;

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffBewegung]') AND name = N'NachFahrzeugId')
                        ALTER TABLE [Lager].[SauerstoffBewegung] ADD [NachFahrzeugId] UNIQUEIDENTIFIER NULL;

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffBewegung]') AND name = N'VonLagerort')
                        ALTER TABLE [Lager].[SauerstoffBewegung] DROP COLUMN [VonLagerort];

                    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Lager].[SauerstoffBewegung]') AND name = N'NachLagerort')
                        ALTER TABLE [Lager].[SauerstoffBewegung] DROP COLUMN [NachLagerort];
                END;

                -- FK sicherstellen
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Lager_SauerstoffBewegung_Flasche')
                    EXEC('ALTER TABLE [Lager].[SauerstoffBewegung]
                              ADD CONSTRAINT [FK_Lager_SauerstoffBewegung_Flasche]
                                  FOREIGN KEY ([FlascheId]) REFERENCES [Lager].[SauerstoffFlasche]([Id])');
            END;
            """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
