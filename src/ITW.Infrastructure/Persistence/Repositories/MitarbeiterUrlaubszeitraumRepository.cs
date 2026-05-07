using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class MitarbeiterUrlaubszeitraumRepository : IMitarbeiterUrlaubszeitraumRepository
{
    private readonly PlatformDbContext _dbContext;

    public MitarbeiterUrlaubszeitraumRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<MitarbeiterUrlaubszeitraum?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1)
                Id, UserId, Von, Bis, Notiz, IstAktiv, ErstelltAmUtc, AktualisiertAmUtc,
                Status, EingereichtVonUserId, Begruendung, Loesung, EntschiedenAm, EntschiedenVonUserId
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE Id = @Id;
            """;

        AddParameter(command, "@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<MitarbeiterUrlaubszeitraum>();
        }

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                Id, UserId, Von, Bis, Notiz, IstAktiv, ErstelltAmUtc, AktualisiertAmUtc,
                Status, EingereichtVonUserId, Begruendung, Loesung, EntschiedenAm, EntschiedenVonUserId
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE UserId = @UserId
              AND IstAktiv = 1
            ORDER BY Von, Bis;
            """;

        AddParameter(command, "@UserId", userId.Trim());

        return await ReadListAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerUndJahrAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<MitarbeiterUrlaubszeitraum>();
        }

        var start = new DateOnly(jahr, 1, 1).ToDateTime(TimeOnly.MinValue);
        var ende  = new DateOnly(jahr, 12, 31).ToDateTime(TimeOnly.MinValue);

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                Id, UserId, Von, Bis, Notiz, IstAktiv, ErstelltAmUtc, AktualisiertAmUtc,
                Status, EingereichtVonUserId, Begruendung, Loesung, EntschiedenAm, EntschiedenVonUserId
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE UserId = @UserId
              AND IstAktiv = 1
              AND Status IN (0, 1)
              AND Von <= @JahresEnde
              AND Bis >= @JahresStart
            ORDER BY Von, Bis;
            """;

        AddParameter(command, "@UserId", userId.Trim());
        AddParameter(command, "@JahresStart", start);
        AddParameter(command, "@JahresEnde", ende);

        return await ReadListAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAllAusstehendAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                Id, UserId, Von, Bis, Notiz, IstAktiv, ErstelltAmUtc, AktualisiertAmUtc,
                Status, EingereichtVonUserId, Begruendung, Loesung, EntschiedenAm, EntschiedenVonUserId
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE IstAktiv = 1
              AND Status = 1
            ORDER BY Von, UserId;
            """;

        return await ReadListAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
        DateOnly datum,
        CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT UserId
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE IstAktiv = 1
              AND Status IN (0, 1)
              AND Von <= @Datum
              AND Bis >= @Datum;
            """;

        AddParameter(command, "@Datum", datum.ToDateTime(TimeOnly.MinValue));

        var result = new List<string>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
            {
                result.Add(reader.GetString(0));
            }
        }

        return result;
    }

    public async Task<bool> HatUeberschneidungAsync(
        string userId,
        DateOnly von,
        DateOnly bis,
        Guid? ausnahmeId = null,
        CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) 1
            FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE UserId = @UserId
              AND IstAktiv = 1
              AND Status IN (0, 1)
              AND Von <= @Bis
              AND Bis >= @Von
              AND (@AusnahmeId IS NULL OR Id <> @AusnahmeId);
            """;

        AddParameter(command, "@UserId", userId.Trim());
        AddParameter(command, "@Von", von.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@Bis", bis.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@AusnahmeId", ausnahmeId.HasValue ? ausnahmeId.Value : DBNull.Value);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is not null && scalar != DBNull.Value;
    }

    public async Task AddOrUpdateAsync(
        MitarbeiterUrlaubszeitraum zeitraum,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zeitraum);

        var connection = _dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF EXISTS (SELECT 1 FROM [Personnel].[MitarbeiterUrlaubszeitraum] WHERE Id = @Id)
            BEGIN
                UPDATE [Personnel].[MitarbeiterUrlaubszeitraum]
                SET UserId                = @UserId,
                    Von                   = @Von,
                    Bis                   = @Bis,
                    Notiz                 = @Notiz,
                    IstAktiv              = @IstAktiv,
                    AktualisiertAmUtc     = @AktualisiertAmUtc,
                    Status                = @Status,
                    EingereichtVonUserId  = @EingereichtVonUserId,
                    Begruendung           = @Begruendung,
                    Loesung               = @Loesung,
                    EntschiedenAm         = @EntschiedenAm,
                    EntschiedenVonUserId  = @EntschiedenVonUserId
                WHERE Id = @Id;
            END
            ELSE
            BEGIN
                INSERT INTO [Personnel].[MitarbeiterUrlaubszeitraum]
                (
                    Id, UserId, Von, Bis, Notiz, IstAktiv, ErstelltAmUtc, AktualisiertAmUtc,
                    Status, EingereichtVonUserId, Begruendung, Loesung, EntschiedenAm, EntschiedenVonUserId
                )
                VALUES
                (
                    @Id, @UserId, @Von, @Bis, @Notiz, @IstAktiv, @ErstelltAmUtc, @AktualisiertAmUtc,
                    @Status, @EingereichtVonUserId, @Begruendung, @Loesung, @EntschiedenAm, @EntschiedenVonUserId
                );
            END
            """;

        AddParameter(command, "@Id",                   zeitraum.Id);
        AddParameter(command, "@UserId",               zeitraum.UserId);
        AddParameter(command, "@Von",                  zeitraum.Von.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@Bis",                  zeitraum.Bis.ToDateTime(TimeOnly.MinValue));
        AddParameter(command, "@Notiz",                zeitraum.Notiz);
        AddParameter(command, "@IstAktiv",             zeitraum.IstAktiv);
        AddParameter(command, "@ErstelltAmUtc",        zeitraum.ErstelltAmUtc);
        AddParameter(command, "@AktualisiertAmUtc",    zeitraum.AktualisiertAmUtc);
        AddParameter(command, "@Status",               (int)zeitraum.Status);
        AddParameter(command, "@EingereichtVonUserId", zeitraum.EingereichtVonUserId);
        AddParameter(command, "@Begruendung",          zeitraum.Begruendung);
        AddParameter(command, "@Loesung",              zeitraum.Loesung);
        AddParameter(command, "@EntschiedenAm",        zeitraum.EntschiedenAm.HasValue ? zeitraum.EntschiedenAm.Value : DBNull.Value);
        AddParameter(command, "@EntschiedenVonUserId", zeitraum.EntschiedenVonUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            DELETE FROM [Personnel].[MitarbeiterUrlaubszeitraum]
            WHERE Id = {0};
            """,
            [id],
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    private static async Task<List<MitarbeiterUrlaubszeitraum>> ReadListAsync(
        DbCommand command,
        CancellationToken cancellationToken)
    {
        var result = new List<MitarbeiterUrlaubszeitraum>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(Map(reader));
        }

        return result;
    }

    private static MitarbeiterUrlaubszeitraum Map(DbDataReader reader)
    {
        return new MitarbeiterUrlaubszeitraum
        {
            Id                   = reader.GetGuid(0),
            UserId               = reader.GetString(1),
            Von                  = DateOnly.FromDateTime(reader.GetDateTime(2)),
            Bis                  = DateOnly.FromDateTime(reader.GetDateTime(3)),
            Notiz                = reader.IsDBNull(4)  ? null : reader.GetString(4),
            IstAktiv             = reader.GetBoolean(5),
            ErstelltAmUtc        = reader.GetFieldValue<DateTimeOffset>(6),
            AktualisiertAmUtc    = reader.GetFieldValue<DateTimeOffset>(7),
            Status               = (UrlaubszeitraumStatus)reader.GetInt32(8),
            EingereichtVonUserId = reader.IsDBNull(9)  ? null : reader.GetString(9),
            Begruendung          = reader.IsDBNull(10) ? null : reader.GetString(10),
            Loesung              = reader.IsDBNull(11) ? null : reader.GetString(11),
            EntschiedenAm        = reader.IsDBNull(12) ? null : reader.GetFieldValue<DateTimeOffset>(12),
            EntschiedenVonUserId = reader.IsDBNull(13) ? null : reader.GetString(13)
        };
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
