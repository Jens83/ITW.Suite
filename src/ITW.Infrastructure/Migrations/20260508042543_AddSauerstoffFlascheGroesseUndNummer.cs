using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITW.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSauerstoffFlascheGroesseUndNummer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlaschenNummer",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Groesse",
                schema: "Lager",
                table: "SauerstoffFlasche",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateIndex(
                name: "UX_Lager_SauerstoffFlasche_FlaschenNummer",
                schema: "Lager",
                table: "SauerstoffFlasche",
                column: "FlaschenNummer",
                unique: true,
                filter: "[FlaschenNummer] IS NOT NULL");

            // Idempotent: Bootstrapper may have already created this FK
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'FK_Lager_SauerstoffBewegung_Flasche', N'F') IS NULL
                    ALTER TABLE [Lager].[SauerstoffBewegung]
                        ADD CONSTRAINT [FK_Lager_SauerstoffBewegung_Flasche]
                            FOREIGN KEY ([FlascheId]) REFERENCES [Lager].[SauerstoffFlasche]([Id])
                            ON DELETE NO ACTION;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'FK_Lager_SauerstoffBewegung_Flasche', N'F') IS NOT NULL
                    ALTER TABLE [Lager].[SauerstoffBewegung]
                        DROP CONSTRAINT [FK_Lager_SauerstoffBewegung_Flasche];
                """);

            migrationBuilder.DropIndex(
                name: "UX_Lager_SauerstoffFlasche_FlaschenNummer",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "FlaschenNummer",
                schema: "Lager",
                table: "SauerstoffFlasche");

            migrationBuilder.DropColumn(
                name: "Groesse",
                schema: "Lager",
                table: "SauerstoffFlasche");
        }
    }
}
