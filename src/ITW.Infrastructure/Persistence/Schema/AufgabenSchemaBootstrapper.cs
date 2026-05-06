using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class AufgabenSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public AufgabenSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureAufgabenSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[Aufgaben]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Aufgaben]
            (
                [Id]               UNIQUEIDENTIFIER  NOT NULL,
                [Bereich]          INT               NOT NULL,
                [Titel]            NVARCHAR(300)     NOT NULL,
                [Prioritaet]       INT               NOT NULL,
                [Status]           INT               NOT NULL,
                [Quelle]           INT               NOT NULL,
                [SystemSchluessel] NVARCHAR(200)     NULL,
                [Faelligkeitsdatum] DATE             NULL,
                [ErstelltAm]       DATETIMEOFFSET    NOT NULL,
                [ErledigtAm]       DATETIMEOFFSET    NULL,
                CONSTRAINT [PK_Aufgaben] PRIMARY KEY ([Id])
            );

            CREATE INDEX [IX_Aufgaben_Bereich_Status]
                ON [dbo].[Aufgaben] ([Bereich], [Status]);

            CREATE INDEX [IX_Aufgaben_SystemSchluessel]
                ON [dbo].[Aufgaben] ([SystemSchluessel]);
        END
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
