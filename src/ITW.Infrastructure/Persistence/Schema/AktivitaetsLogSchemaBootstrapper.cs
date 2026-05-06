using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class AktivitaetsLogSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public AktivitaetsLogSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureAktivitaetsLogSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[AktivitaetsLog]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[AktivitaetsLog]
            (
                [Id]          UNIQUEIDENTIFIER    NOT NULL,
                [Bereich]     INT                 NOT NULL,
                [Text]        NVARCHAR(500)       NOT NULL,
                [Kategorie]   INT                 NOT NULL,
                [IconCssClass] NVARCHAR(100)      NOT NULL,
                [Zeitpunkt]   DATETIMEOFFSET      NOT NULL,
                CONSTRAINT [PK_AktivitaetsLog] PRIMARY KEY ([Id])
            );

            CREATE INDEX [IX_AktivitaetsLog_Bereich_Zeitpunkt]
                ON [dbo].[AktivitaetsLog] ([Bereich], [Zeitpunkt]);
        END
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
