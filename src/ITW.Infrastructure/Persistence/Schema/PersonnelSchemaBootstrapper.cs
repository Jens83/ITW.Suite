using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class PersonnelSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public PersonnelSchemaBootstrapper(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task EnsurePersonnelSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[AllgemeineMitarbeiterprofile]', N'U') IS NOT NULL
           AND COL_LENGTH(N'[dbo].[AllgemeineMitarbeiterprofile]', N'Beschaeftigungsart') IS NULL
        BEGIN
            ALTER TABLE [dbo].[AllgemeineMitarbeiterprofile]
            ADD [Beschaeftigungsart] INT NOT NULL
                CONSTRAINT [DF_AllgemeineMitarbeiterprofile_Beschaeftigungsart] DEFAULT (0);
        END;
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}