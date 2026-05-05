using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Schema;

public sealed class DienstplanSchemaBootstrapper
{
    private readonly PlatformDbContext _dbContext;

    public DienstplanSchemaBootstrapper(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public Task EnsureDienstplanSchemaAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
        IF OBJECT_ID(N'[dbo].[DienstplanPerioden]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[DienstplanPerioden]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [Jahr] INT NOT NULL,
                [Monat] INT NOT NULL,
                [Bezeichnung] NVARCHAR(100) NOT NULL,
                [WunschphaseIstOffen] BIT NOT NULL,
                [PlanIstFreigegeben] BIT NOT NULL CONSTRAINT [DF_DienstplanPerioden_PlanIstFreigegeben] DEFAULT (0),
                [PlanFreigegebenAm] DATETIMEOFFSET(7) NULL,
                [PlanFreigegebenVonUserId] NVARCHAR(450) NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [ErstelltVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_DienstplanPerioden] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_DienstplanPerioden_Jahr_Monat'
              AND object_id = OBJECT_ID(N'[dbo].[DienstplanPerioden]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_DienstplanPerioden_Jahr_Monat]
                ON [dbo].[DienstplanPerioden] ([Jahr], [Monat]);
        END;

                IF COL_LENGTH(N'[dbo].[DienstplanPerioden]', N'PlanIstFreigegeben') IS NULL
        BEGIN
            ALTER TABLE [dbo].[DienstplanPerioden]
            ADD [PlanIstFreigegeben] BIT NOT NULL
                CONSTRAINT [DF_DienstplanPerioden_PlanIstFreigegeben] DEFAULT (0);
        END;

        IF COL_LENGTH(N'[dbo].[DienstplanPerioden]', N'PlanFreigegebenAm') IS NULL
        BEGIN
            ALTER TABLE [dbo].[DienstplanPerioden]
            ADD [PlanFreigegebenAm] DATETIMEOFFSET(7) NULL;
        END;

        IF COL_LENGTH(N'[dbo].[DienstplanPerioden]', N'PlanFreigegebenVonUserId') IS NULL
        BEGIN
            ALTER TABLE [dbo].[DienstplanPerioden]
            ADD [PlanFreigegebenVonUserId] NVARCHAR(450) NULL;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_DienstplanPerioden_PlanIstFreigegeben'
              AND object_id = OBJECT_ID(N'[dbo].[DienstplanPerioden]')
        )
        BEGIN
            CREATE INDEX [IX_DienstplanPerioden_PlanIstFreigegeben]
                ON [dbo].[DienstplanPerioden] ([PlanIstFreigegeben]);
        END;

        IF OBJECT_ID(N'[dbo].[DienstplanWuensche]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[DienstplanWuensche]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DienstplanPeriodeId] UNIQUEIDENTIFIER NOT NULL,
                [UserId] NVARCHAR(450) NOT NULL,
                [WunschDatum] DATE NOT NULL,
                [WunschTyp] INT NOT NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                CONSTRAINT [PK_DienstplanWuensche] PRIMARY KEY ([Id])
            );
        END;

        IF COL_LENGTH(N'[dbo].[DienstplanWuensche]', N'WunschTyp') IS NULL
        BEGIN
            ALTER TABLE [dbo].[DienstplanWuensche]
            ADD [WunschTyp] INT NOT NULL
                CONSTRAINT [DF_DienstplanWuensche_WunschTyp] DEFAULT (1);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_DienstplanWuensche_Periode_User_Datum'
              AND object_id = OBJECT_ID(N'[dbo].[DienstplanWuensche]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_DienstplanWuensche_Periode_User_Datum]
                ON [dbo].[DienstplanWuensche] ([DienstplanPeriodeId], [UserId], [WunschDatum]);
        END;

        IF OBJECT_ID(N'[dbo].[FreelancerMonatswuensche]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[FreelancerMonatswuensche]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DienstplanPeriodeId] UNIQUEIDENTIFIER NOT NULL,
                [UserId] NVARCHAR(450) NOT NULL,
                [GewuenschteDienste] INT NOT NULL,
                [ErstelltAm] DATETIMEOFFSET(7) NOT NULL,
                [AktualisiertAm] DATETIMEOFFSET(7) NOT NULL,
                CONSTRAINT [PK_FreelancerMonatswuensche] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_FreelancerMonatswuensche_Periode_User'
              AND object_id = OBJECT_ID(N'[dbo].[FreelancerMonatswuensche]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_FreelancerMonatswuensche_Periode_User]
                ON [dbo].[FreelancerMonatswuensche] ([DienstplanPeriodeId], [UserId]);
        END;

        IF OBJECT_ID(N'[dbo].[GeplanteDiensttage]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[GeplanteDiensttage]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DienstplanPeriodeId] UNIQUEIDENTIFIER NOT NULL,
                [DienstDatum] DATE NOT NULL,
                [ArztUserId] NVARCHAR(450) NULL,
                [Notfallsanitaeter1UserId] NVARCHAR(450) NULL,
                [Notfallsanitaeter2UserId] NVARCHAR(450) NULL,
                [AktualisiertAm] DATETIMEOFFSET(7) NOT NULL,
                [AktualisiertVonUserId] NVARCHAR(450) NOT NULL,
                CONSTRAINT [PK_GeplanteDiensttage] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_GeplanteDiensttage_Periode_Datum'
              AND object_id = OBJECT_ID(N'[dbo].[GeplanteDiensttage]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_GeplanteDiensttage_Periode_Datum]
                ON [dbo].[GeplanteDiensttage] ([DienstplanPeriodeId], [DienstDatum]);
        END;

        IF OBJECT_ID(N'[dbo].[GeplanteDiensttagAusfaelle]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[GeplanteDiensttagAusfaelle]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DienstplanPeriodeId] UNIQUEIDENTIFIER NOT NULL,
                [DienstDatum] DATE NOT NULL,
                [BesetzungsSlotCode] INT NOT NULL,
                [UrspruenglichGeplanterUserId] NVARCHAR(450) NOT NULL,
                [AusfallGrundCode] INT NOT NULL,
                [VertretungsUserId] NVARCHAR(450) NULL,
                [ErfasstVonUserId] NVARCHAR(450) NOT NULL,
                [ErfasstAm] DATETIMEOFFSET(7) NOT NULL,
                CONSTRAINT [PK_GeplanteDiensttagAusfaelle] PRIMARY KEY ([Id])
            );
        END;

                IF OBJECT_ID(N'[dbo].[AutoplanLernereignisse]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[AutoplanLernereignisse]
            (
                [Id] UNIQUEIDENTIFIER NOT NULL,
                [DienstplanPeriodeId] UNIQUEIDENTIFIER NOT NULL,
                [DienstDatum] DATE NOT NULL,
                [BesetzungsSlotCode] INT NOT NULL,
                [EreignisTypCode] INT NOT NULL,
                [VorherigeUserId] NVARCHAR(450) NULL,
                [NeueUserId] NVARCHAR(450) NULL,
                [UrspruenglichGeplanterUserId] NVARCHAR(450) NULL,
                [KontextArztUserId] NVARCHAR(450) NULL,
                [KontextNotfallsanitaeter1UserId] NVARCHAR(450) NULL,
                [KontextNotfallsanitaeter2UserId] NVARCHAR(450) NULL,
                [AusfallGrundCode] INT NULL,
                [BearbeitetVonUserId] NVARCHAR(450) NOT NULL,
                [ErfasstAm] DATETIMEOFFSET(7) NOT NULL,
                CONSTRAINT [PK_AutoplanLernereignisse] PRIMARY KEY ([Id])
            );
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_AutoplanLernereignisse_Periode_Datum'
              AND object_id = OBJECT_ID(N'[dbo].[AutoplanLernereignisse]')
        )
        BEGIN
            CREATE INDEX [IX_AutoplanLernereignisse_Periode_Datum]
                ON [dbo].[AutoplanLernereignisse] ([DienstplanPeriodeId], [DienstDatum]);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_AutoplanLernereignisse_ErfasstAm'
              AND object_id = OBJECT_ID(N'[dbo].[AutoplanLernereignisse]')
        )
        BEGIN
            CREATE INDEX [IX_AutoplanLernereignisse_ErfasstAm]
                ON [dbo].[AutoplanLernereignisse] ([ErfasstAm]);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'IX_GeplanteDiensttagAusfaelle_Periode_Datum_Slot'
              AND object_id = OBJECT_ID(N'[dbo].[GeplanteDiensttagAusfaelle]')
        )
        BEGIN
            CREATE UNIQUE INDEX [IX_GeplanteDiensttagAusfaelle_Periode_Datum_Slot]
                ON [dbo].[GeplanteDiensttagAusfaelle] ([DienstplanPeriodeId], [DienstDatum], [BesetzungsSlotCode]);
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_DienstplanWuensche_DienstplanPerioden_DienstplanPeriodeId'
        )
        BEGIN
            ALTER TABLE [dbo].[DienstplanWuensche]
            ADD CONSTRAINT [FK_DienstplanWuensche_DienstplanPerioden_DienstplanPeriodeId]
            FOREIGN KEY ([DienstplanPeriodeId])
            REFERENCES [dbo].[DienstplanPerioden] ([Id])
            ON DELETE CASCADE;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_FreelancerMonatswuensche_DienstplanPerioden_DienstplanPeriodeId'
        )
        BEGIN
            ALTER TABLE [dbo].[FreelancerMonatswuensche]
            ADD CONSTRAINT [FK_FreelancerMonatswuensche_DienstplanPerioden_DienstplanPeriodeId]
            FOREIGN KEY ([DienstplanPeriodeId])
            REFERENCES [dbo].[DienstplanPerioden] ([Id])
            ON DELETE CASCADE;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_GeplanteDiensttage_DienstplanPerioden_DienstplanPeriodeId'
        )
        BEGIN
            ALTER TABLE [dbo].[GeplanteDiensttage]
            ADD CONSTRAINT [FK_GeplanteDiensttage_DienstplanPerioden_DienstplanPeriodeId]
            FOREIGN KEY ([DienstplanPeriodeId])
            REFERENCES [dbo].[DienstplanPerioden] ([Id])
            ON DELETE CASCADE;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_GeplanteDiensttagAusfaelle_DienstplanPerioden_DienstplanPeriodeId'
        )
        BEGIN
            ALTER TABLE [dbo].[GeplanteDiensttagAusfaelle]
            ADD CONSTRAINT [FK_GeplanteDiensttagAusfaelle_DienstplanPerioden_DienstplanPeriodeId]
            FOREIGN KEY ([DienstplanPeriodeId])
            REFERENCES [dbo].[DienstplanPerioden] ([Id])
            ON DELETE CASCADE;
        END;

                IF NOT EXISTS (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_AutoplanLernereignisse_DienstplanPerioden_DienstplanPeriodeId'
        )
        BEGIN
            ALTER TABLE [dbo].[AutoplanLernereignisse]
            ADD CONSTRAINT [FK_AutoplanLernereignisse_DienstplanPerioden_DienstplanPeriodeId]
            FOREIGN KEY ([DienstplanPeriodeId])
            REFERENCES [dbo].[DienstplanPerioden] ([Id])
            ON DELETE CASCADE;
        END;
        """;

        return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}