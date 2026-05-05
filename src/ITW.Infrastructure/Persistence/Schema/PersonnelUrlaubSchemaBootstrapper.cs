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
                    [AktualisiertAmUtc] DATETIMEOFFSET NOT NULL
                );

                CREATE INDEX [IX_MitarbeiterUrlaubszeitraum_UserId_Von_Bis]
                    ON [Personnel].[MitarbeiterUrlaubszeitraum] ([UserId], [Von], [Bis]);
            END
            """,
            cancellationToken);
    }
}