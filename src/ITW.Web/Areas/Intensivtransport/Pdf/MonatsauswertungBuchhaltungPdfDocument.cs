using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ITW.Web.Areas.Intensivtransport.Pdf;

public sealed class MonatsauswertungBuchhaltungPdfDocument
{
    private readonly MonatsauswertungViewModel _model;

    public MonatsauswertungBuchhaltungPdfDocument(MonatsauswertungViewModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public byte[] Generate()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document
            .Create(Compose)
            .WithMetadata(new DocumentMetadata
            {
                Title = $"Buchhaltung Monatsauswertung {_model.AusgewaehltePeriodeBezeichnung}",
                Author = "ITW.Suite",
                Subject = "Dienstplan Monatsauswertung für die Buchhaltung",
                Creator = "ITW.Web",
                CreationDate = DateTimeOffset.Now,
                ModifiedDate = DateTimeOffset.Now,
                Language = "de-DE"
            })
            .WithSettings(new DocumentSettings
            {
                CompressDocument = true,
                ImageCompressionQuality = ImageCompressionQuality.High
            });

        return document.GeneratePdf();
    }

    private void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(8.5f).FontColor(Colors.Black));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Seite ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(3);

            column.Item()
                .Text("Monatsauswertung")
                .FontSize(20)
                .SemiBold();

            column.Item()
                .Text("Übersicht Intensivtransport")
                .FontSize(10)
                .FontColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(12).Column(column =>
        {
            column.Spacing(12);

            column.Item().Element(ComposeInfoBox);
            column.Item().Element(ComposeKennzahlen);
            column.Item().Element(ComposeMonatsinfo);
            column.Item().Element(ComposeMitarbeiterTabelle);
            column.Item().Element(ComposeFachhinweis);
        });
    }

    private void ComposeInfoBox(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(10)
            .Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text("Periode").FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                    column.Item().Text(_model.AusgewaehltePeriodeBezeichnung);
                });

                row.RelativeItem().Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text("Exportdatum").FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                    column.Item().Text(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                });

                row.RelativeItem().Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text("Mitarbeiter").FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                    column.Item().Text(_model.Mitarbeiter.Count.ToString());
                });
            });
    }

    private void ComposeKennzahlen(IContainer container)
    {
        container.Row(row =>
        {
            row.Spacing(8);

            row.RelativeItem().Element(x => ComposeKennzahlKarte(x, "Geplant", _model.SummeGeplanteDienste));
            row.RelativeItem().Element(x => ComposeKennzahlKarte(x, "Gefahren", _model.SummeGefahreneDienste));
            row.RelativeItem().Element(x => ComposeKennzahlKarte(x, "Vertretungen", _model.SummeVertretungen));
            row.RelativeItem().Element(x => ComposeKennzahlKarte(x, "Gesamt", _model.SummeGesamt));
        });
    }

    private void ComposeMonatsinfo(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten4)
            .Padding(10)
            .Row(row =>
            {
                row.RelativeItem().Text($"Krankheitstage im Monat: {_model.SummeKrankheitstage}")
                    .FontSize(9)
                    .SemiBold();

                row.RelativeItem().AlignRight().Text($"Urlaubstage im Monat: {_model.SummeUrlaubstage}")
                    .FontSize(9)
                    .SemiBold();
            });
    }

    private static void ComposeKennzahlKarte(IContainer container, string titel, int wert)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten4)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(2);
                column.Item().Text(titel).FontSize(8).SemiBold().FontColor(Colors.Grey.Medium);
                column.Item().Text(wert.ToString()).FontSize(16).SemiBold();
            });
    }

    private void ComposeMitarbeiterTabelle(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(6);

            column.Item()
                .Text("Mitarbeiterübersicht")
                .FontSize(12)
                .SemiBold();

            column.Item().Table(table =>
            {
                DefiniereSpalten(table);

                table.Header(header =>
                {
                    HeaderText(header.Cell(), "Mitarbeiter / Qualifikation");
                    HeaderText(header.Cell(), "Geplant");
                    HeaderText(header.Cell(), "Gefahren");
                    HeaderText(header.Cell(), "Vertretung");
                    HeaderText(header.Cell(), "Gesamt");
                    HeaderText(header.Cell(), "Krank");
                    HeaderText(header.Cell(), "Urlaub Monat");
                    HeaderText(header.Cell(), "Anspruch");
                    HeaderText(header.Cell(), "Genommen");
                    HeaderText(header.Cell(), "Resturlaub");
                });

                var sortierteMitarbeiter = _model.Mitarbeiter
                    .OrderBy(x => HoleQualifikationsRang(x.Hauptqualifikation))
                    .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase);

                foreach (var eintrag in sortierteMitarbeiter)
                {
                    table.Cell().Element(BodyCell).ShowEntire().Column(col =>
                    {
                        col.Spacing(1);
                        col.Item().Text(string.IsNullOrWhiteSpace(eintrag.AnzeigeName) ? eintrag.UserId : eintrag.AnzeigeName).SemiBold();

                        if (!string.IsNullOrWhiteSpace(eintrag.Hauptqualifikation))
                        {
                            col.Item().Text(eintrag.Hauptqualifikation).FontSize(8).FontColor(Colors.Grey.Medium);
                        }
                    });

                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.GeplanteDienste);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.GefahreneDienste);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Vertretungen);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Gesamt);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Krankheitstage);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Urlaubstage);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Jahresurlaubsanspruch);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.GenommeneUrlaubstageImJahr);
                    NumberText(table.Cell().Element(BodyCell).ShowEntire(), eintrag.Resturlaubstage);
                }

                table.Cell().Element(TotalCell).Text("Gesamtsumme").SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeGeplanteDienste.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeGefahreneDienste.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeVertretungen.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeGesamt.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeKrankheitstage.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text(_model.SummeUrlaubstage.ToString()).SemiBold();
                table.Cell().Element(TotalCell).AlignCenter().Text("-").SemiBold().FontColor(Colors.Grey.Medium);
                table.Cell().Element(TotalCell).AlignCenter().Text("-").SemiBold().FontColor(Colors.Grey.Medium);
                table.Cell().Element(TotalCell).AlignCenter().Text("-").SemiBold().FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static int HoleQualifikationsRang(string qualifikation)
    {
        if (string.IsNullOrWhiteSpace(qualifikation))
        {
            return 99;
        }

        var q = qualifikation.ToLower();
        if (q.Contains("arzt") || q.Contains("ärztin"))
        {
            return 1;
        }

        if (q.Contains("notfallsanitäter"))
        {
            return 2;
        }

        if (q.Contains("rettungssanitäter"))
        {
            return 3;
        }

        return 10;
    }

    private void ComposeFachhinweis(IContainer container)
    {
        const string hinweis = "Datengrundlage: Monatsauswertung aus dem Dienstplan plus Jahresurlaub je Mitarbeiter aus dem Urlaubsplaner.";

        container
            .PaddingTop(4)
            .Text(hinweis)
            .FontSize(8)
            .FontColor(Colors.Grey.Medium);
    }

    private static void DefiniereSpalten(TableDescriptor table)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(3.8f);
            columns.ConstantColumn(52);
            columns.ConstantColumn(52);
            columns.ConstantColumn(58);
            columns.ConstantColumn(52);
            columns.ConstantColumn(48);
            columns.ConstantColumn(60);
            columns.ConstantColumn(56);
            columns.ConstantColumn(58);
            columns.ConstantColumn(58);
        });
    }

    private static void HeaderText(IContainer container, string text)
    {
        container
            .Element(HeaderCell)
            .AlignCenter()
            .Text(text)
            .SemiBold();
    }

    private static void NumberText(IContainer container, int value)
    {
        container
            .AlignCenter()
            .Text(value.ToString());
    }

    private static IContainer HeaderCell(IContainer container)
        => container
            .Background(Colors.Grey.Lighten3)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(6);

    private static IContainer BodyCell(IContainer container)
        => container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(6)
            .PaddingHorizontal(6);

    private static IContainer TotalCell(IContainer container)
        => container
            .Background(Colors.Grey.Lighten4)
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(8)
            .PaddingHorizontal(6);
}