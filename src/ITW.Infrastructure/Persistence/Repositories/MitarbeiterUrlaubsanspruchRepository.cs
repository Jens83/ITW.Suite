using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class MitarbeiterUrlaubsanspruchRepository : IMitarbeiterUrlaubsanspruchRepository
{
    private readonly PlatformDbContext _dbContext;

    public MitarbeiterUrlaubsanspruchRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<MitarbeiterUrlaubsanspruch?> GetAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1)
                Id,
                UserId,
                Jahr,
                Anspruchstage,
                Uebertragstage,
                Bemerkung,
                ErstelltAmUtc,
                AktualisiertAmUtc
            FROM [Personnel].[MitarbeiterUrlaubsanspruch]
            WHERE UserId = @UserId
              AND Jahr = @Jahr;
            """;

        AddParameter(command, "@UserId", userId.Trim());
        AddParameter(command, "@Jahr", jahr);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new MitarbeiterUrlaubsanspruch
        {
            Id = reader.GetGuid(0),
            UserId = reader.GetString(1),
            Jahr = reader.GetInt32(2),
            Anspruchstage = reader.GetInt32(3),
            Uebertragstage = reader.GetInt32(4),
            Bemerkung = reader.IsDBNull(5) ? null : reader.GetString(5),
            ErstelltAmUtc = reader.GetFieldValue<DateTimeOffset>(6),
            AktualisiertAmUtc = reader.GetFieldValue<DateTimeOffset>(7)
        };
    }

    public async Task<IReadOnlyList<MitarbeiterUrlaubsanspruch>> GetAlleFuerBenutzerAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<MitarbeiterUrlaubsanspruch>();
        }

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                Id,
                UserId,
                Jahr,
                Anspruchstage,
                Uebertragstage,
                Bemerkung,
                ErstelltAmUtc,
                AktualisiertAmUtc
            FROM [Personnel].[MitarbeiterUrlaubsanspruch]
            WHERE UserId = @UserId
            ORDER BY Jahr DESC;
            """;

        AddParameter(command, "@UserId", userId.Trim());

        var result = new List<MitarbeiterUrlaubsanspruch>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new MitarbeiterUrlaubsanspruch
            {
                Id = reader.GetGuid(0),
                UserId = reader.GetString(1),
                Jahr = reader.GetInt32(2),
                Anspruchstage = reader.GetInt32(3),
                Uebertragstage = reader.GetInt32(4),
                Bemerkung = reader.IsDBNull(5) ? null : reader.GetString(5),
                ErstelltAmUtc = reader.GetFieldValue<DateTimeOffset>(6),
                AktualisiertAmUtc = reader.GetFieldValue<DateTimeOffset>(7)
            });
        }

        return result;
    }

    public async Task UpsertAsync(
    MitarbeiterUrlaubsanspruch anspruch,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anspruch);

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
        IF EXISTS (
            SELECT 1
            FROM [Personnel].[MitarbeiterUrlaubsanspruch]
            WHERE UserId = @UserId
              AND Jahr = @Jahr
        )
        BEGIN
            UPDATE [Personnel].[MitarbeiterUrlaubsanspruch]
            SET Anspruchstage = @Anspruchstage,
                Uebertragstage = @Uebertragstage,
                Bemerkung = @Bemerkung,
                AktualisiertAmUtc = @AktualisiertAmUtc
            WHERE UserId = @UserId
              AND Jahr = @Jahr;
        END
        ELSE
        BEGIN
            INSERT INTO [Personnel].[MitarbeiterUrlaubsanspruch]
            (
                Id,
                UserId,
                Jahr,
                Anspruchstage,
                Uebertragstage,
                Bemerkung,
                ErstelltAmUtc,
                AktualisiertAmUtc
            )
            VALUES
            (
                @Id,
                @UserId,
                @Jahr,
                @Anspruchstage,
                @Uebertragstage,
                @Bemerkung,
                @ErstelltAmUtc,
                @AktualisiertAmUtc
            );
        END
        """;

        AddParameter(command, "@Id", anspruch.Id);
        AddParameter(command, "@UserId", anspruch.UserId);
        AddParameter(command, "@Jahr", anspruch.Jahr);
        AddParameter(command, "@Anspruchstage", anspruch.Anspruchstage);
        AddParameter(command, "@Uebertragstage", anspruch.Uebertragstage);
        AddParameter(command, "@Bemerkung", anspruch.Bemerkung);
        AddParameter(command, "@ErstelltAmUtc", anspruch.ErstelltAmUtc);
        AddParameter(command, "@AktualisiertAmUtc", anspruch.AktualisiertAmUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureOpenAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}