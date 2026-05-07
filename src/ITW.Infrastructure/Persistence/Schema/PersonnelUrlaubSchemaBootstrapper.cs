using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class PersonnelUrlaubSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public PersonnelUrlaubSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task EnsurePersonnelUrlaubSchemaAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            IF SCHEMA_ID(N'Personnel') IS NULL
            BEGIN
                EXEC(N'CREATE SCHEMA [Personnel]');
            END

            IF OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubsanspruch]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Personnel].[MitarbeiterUrlaubsanspruch]
                (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [UserId] NVARCHAR(450) NOT NULL,
                    [Jahr] INT NOT NULL,
                    [Anspruchstage] INT NOT NULL,
                    [Uebertragstage] INT NOT NULL,
                    [Bemerkung] NVARCHAR(1000) NULL,
                    [ErstelltAmUtc] DATETIMEOFFSET NOT NULL,
                    [AktualisiertAmUtc] DATETIMEOFFSET NOT NULL
                );

                CREATE UNIQUE INDEX [IX_MitarbeiterUrlaubsanspruch_UserId_Jahr]
                    ON [Personnel].[MitarbeiterUrlaubsanspruch] ([UserId], [Jahr]);
            END

            IF OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Personnel].[MitarbeiterUrlaubszeitraum]
                (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [UserId] NVARCHAR(450) NOT NULL,
                    [Von] DATE NOT NULL,
                    [Bis] DATE NOT NULL,
                    [Notiz] NVARCHAR(1000) NULL,
                    [IstAktiv] BIT NOT NULL,
                    [ErstelltAmUtc] DATETIMEOFFSET NOT NULL,
                    [AktualisiertAmUtc] DATETIMEOFFSET NOT NULL,
                    [Status] INT NOT NULL DEFAULT 0,
                    [EingereichtVonUserId] NVARCHAR(256) NULL,
                    [Begruendung] NVARCHAR(1000) NULL,
                    [Loesung] NVARCHAR(500) NULL,
                    [EntschiedenAm] DATETIMEOFFSET NULL,
                    [EntschiedenVonUserId] NVARCHAR(256) NULL,
                    [MitarbeiterBestaetigtAm] DATETIMEOFFSET NULL
                );

                CREATE INDEX [IX_MitarbeiterUrlaubszeitraum_UserId_Von_Bis]
                    ON [Personnel].[MitarbeiterUrlaubszeitraum] ([UserId], [Von], [Bis]);
            END

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'Status')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [Status] INT NOT NULL DEFAULT 0;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'EingereichtVonUserId')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [EingereichtVonUserId] NVARCHAR(256) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'Begruendung')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [Begruendung] NVARCHAR(1000) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'Loesung')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [Loesung] NVARCHAR(500) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'EntschiedenAm')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [EntschiedenAm] DATETIMEOFFSET NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'EntschiedenVonUserId')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [EntschiedenVonUserId] NVARCHAR(256) NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Personnel].[MitarbeiterUrlaubszeitraum]') AND name = N'MitarbeiterBestaetigtAm')
                ALTER TABLE [Personnel].[MitarbeiterUrlaubszeitraum] ADD [MitarbeiterBestaetigtAm] DATETIMEOFFSET NULL;
            """,
            cancellationToken);
    }
}