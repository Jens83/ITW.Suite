using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class OrganisationSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public OrganisationSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureOrganisationSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[ModulZuweisungen]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[ModulZuweisungen]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [Modul] INT NOT NULL,
                [Bereich] INT NOT NULL,
                [Rolle] INT NOT NULL,
                [IstAktiv] BIT NOT NULL,
                [ZugewiesenAm] DATETIMEOFFSET NOT NULL,
                [ZugewiesenVonUserId] NVARCHAR(450) NOT NULL,
                [DeaktiviertAm] DATETIMEOFFSET NULL,
                [DeaktiviertVonUserId] NVARCHAR(450) NULL,
                CONSTRAINT [PK_ModulZuweisungen] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ModulZuweisungen_Modul_Bereich_Rolle'
              AND object_id = OBJECT_ID(N'[dbo].[ModulZuweisungen]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_ModulZuweisungen_Modul_Bereich_Rolle]
                ON [dbo].[ModulZuweisungen] ([Modul], [Bereich], [Rolle]);
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_ModulZuweisungen_Bereich_Rolle_Aktiv'
              AND object_id = OBJECT_ID(N'[dbo].[ModulZuweisungen]')
        )
        BEGIN
            CREATE INDEX [IX_ModulZuweisungen_Bereich_Rolle_Aktiv]
                ON [dbo].[ModulZuweisungen] ([Bereich], [Rolle], [IstAktiv]);
        END;
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}