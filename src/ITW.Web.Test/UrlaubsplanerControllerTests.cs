using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;
using ITW.Domain.Personnel.Qualifications;
using ITW.Web.Areas.Intensivtransport.Controllers;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace ITW.Web.Test;

public sealed class UrlaubsplanerControllerTests
{
    [Fact]
    public async Task ZeitraumSpeichern_EntferntVorhandeneWuenscheImUrlaubszeitraumAusOffenenPerioden()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 10);
        var bis = new DateOnly(2026, 2, 12);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();

        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var februarPeriodeId = Guid.NewGuid();
        var maerzPeriodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden =
            [
                ErzeugePeriode(februarPeriodeId, 2026, 2),
                ErzeugePeriode(maerzPeriodeId, 2026, 3)
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Muss entfernt werden
                ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 10), DienstwunschTyp.Wunsch),
                ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 11), DienstwunschTyp.NichtVerfuegbar),

                // Muss bleiben: gleicher Monat, aber außerhalb Urlaub
                ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 20), DienstwunschTyp.Wunsch),

                // Muss bleiben: andere offene Periode, aber außerhalb Urlaub
                ErzeugeWunsch(maerzPeriodeId, userId, new DateOnly(2026, 3, 5), DienstwunschTyp.Wunsch)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal(userId, redirectResult.RouteValues?["userId"]);
        Assert.Equal(jahr, redirectResult.RouteValues?["jahr"]);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Contains("Urlaubszeitraum wurde hinterlegt", successMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2 gespeicherte Dienstwünsche", successMessage, StringComparison.OrdinalIgnoreCase);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);
        Assert.Equal(userId, urlaubszeitraumRepository.GespeicherteZeitraeume[0].UserId);
        Assert.Equal(von, urlaubszeitraumRepository.GespeicherteZeitraeume[0].Von);
        Assert.Equal(bis, urlaubszeitraumRepository.GespeicherteZeitraeume[0].Bis);

        Assert.Equal(2, dienstwunschRepository.EntfernteWuensche.Count);
        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 2, 10));
        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 2, 11));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 2, 20));
        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 3, 5));

        Assert.Equal(1, dienstwunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ZeitraumSpeichern_LaesstWuenscheUnveraendert_WennKeineImUrlaubszeitraumLiegen()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 10);
        var bis = new DateOnly(2026, 2, 12);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();
        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var februarPeriodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden =
            [
                ErzeugePeriode(februarPeriodeId, 2026, 2)
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 5), DienstwunschTyp.Wunsch),
            ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 20), DienstwunschTyp.NichtVerfuegbar)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal(userId, redirectResult.RouteValues?["userId"]);
        Assert.Equal(jahr, redirectResult.RouteValues?["jahr"]);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Equal("Der Urlaubszeitraum wurde hinterlegt.", successMessage);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);
        Assert.Equal(2, dienstwunschRepository.Wuensche.Count);
        Assert.Empty(dienstwunschRepository.EntfernteWuensche);
        Assert.Equal(0, dienstwunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ZeitraumSpeichern_EntferntBetroffeneWuenscheAuchBeiMonatsuebergreifendemUrlaubAusAllenOffenenPerioden()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 27);
        var bis = new DateOnly(2026, 3, 2);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();
        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var februarPeriodeId = Guid.NewGuid();
        var maerzPeriodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden =
            [
                ErzeugePeriode(februarPeriodeId, 2026, 2),
            ErzeugePeriode(maerzPeriodeId, 2026, 3)
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Februar - betroffen
                ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 27), DienstwunschTyp.Wunsch),
            ErzeugeWunsch(februarPeriodeId, userId, new DateOnly(2026, 2, 28), DienstwunschTyp.NichtVerfuegbar),

            // März - betroffen
            ErzeugeWunsch(maerzPeriodeId, userId, new DateOnly(2026, 3, 1), DienstwunschTyp.Wunsch),
            ErzeugeWunsch(maerzPeriodeId, userId, new DateOnly(2026, 3, 2), DienstwunschTyp.NichtVerfuegbar),

            // März - außerhalb Urlaub, muss bleiben
            ErzeugeWunsch(maerzPeriodeId, userId, new DateOnly(2026, 3, 10), DienstwunschTyp.Wunsch)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal(userId, redirectResult.RouteValues?["userId"]);
        Assert.Equal(jahr, redirectResult.RouteValues?["jahr"]);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Contains("4 gespeicherte Dienstwünsche", successMessage, StringComparison.OrdinalIgnoreCase);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);

        Assert.Equal(4, dienstwunschRepository.EntfernteWuensche.Count);
        Assert.Equal(1, dienstwunschRepository.Wuensche.Count);

        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 2, 27));
        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 2, 28));
        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 3, 1));
        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 3, 2));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.WunschDatum == new DateOnly(2026, 3, 10));

        Assert.Equal(2, dienstwunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ZeitraumSpeichern_EntferntNurWuenscheAusOffenenPerioden()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 10);
        var bis = new DateOnly(2026, 2, 12);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();
        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var offenePeriodeId = Guid.NewGuid();
        var geschlossenePeriodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden =
            [
                ErzeugePeriode(offenePeriodeId, 2026, 2)
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Offen -> muss entfernt werden
                ErzeugeWunsch(offenePeriodeId, userId, new DateOnly(2026, 2, 10), DienstwunschTyp.Wunsch),

            // Nicht in offener Periode -> muss bestehen bleiben
            ErzeugeWunsch(geschlossenePeriodeId, userId, new DateOnly(2026, 2, 11), DienstwunschTyp.Wunsch),

            // Offen, aber außerhalb Urlaub -> muss bleiben
            ErzeugeWunsch(offenePeriodeId, userId, new DateOnly(2026, 2, 20), DienstwunschTyp.NichtVerfuegbar)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Contains("1 gespeicherte Dienstwünsche", successMessage, StringComparison.OrdinalIgnoreCase);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);
        Assert.Single(dienstwunschRepository.EntfernteWuensche);
        Assert.Equal(new DateOnly(2026, 2, 10), dienstwunschRepository.EntfernteWuensche[0].WunschDatum);

        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.DienstplanPeriodeId == offenePeriodeId &&
                 x.WunschDatum == new DateOnly(2026, 2, 10));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.DienstplanPeriodeId == geschlossenePeriodeId &&
                 x.WunschDatum == new DateOnly(2026, 2, 11));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.DienstplanPeriodeId == offenePeriodeId &&
                 x.WunschDatum == new DateOnly(2026, 2, 20));

        Assert.Equal(1, dienstwunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ZeitraumSpeichern_EntferntNurWuenscheDesBetroffenenBenutzers()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var andererUserId = "mitarbeiter-2";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 10);
        var bis = new DateOnly(2026, 2, 12);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();
        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var offenePeriodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden =
            [
                ErzeugePeriode(offenePeriodeId, 2026, 2)
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Betroffener Benutzer im Urlaub -> muss entfernt werden
                ErzeugeWunsch(offenePeriodeId, userId, new DateOnly(2026, 2, 10), DienstwunschTyp.Wunsch),

            // Anderer Benutzer im selben Zeitraum -> muss bleiben
            ErzeugeWunsch(offenePeriodeId, andererUserId, new DateOnly(2026, 2, 10), DienstwunschTyp.Wunsch),

            // Betroffener Benutzer außerhalb Urlaub -> muss bleiben
            ErzeugeWunsch(offenePeriodeId, userId, new DateOnly(2026, 2, 20), DienstwunschTyp.NichtVerfuegbar)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Contains("1 gespeicherte Dienstwünsche", successMessage, StringComparison.OrdinalIgnoreCase);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);
        Assert.Single(dienstwunschRepository.EntfernteWuensche);
        Assert.Equal(userId, dienstwunschRepository.EntfernteWuensche[0].UserId);
        Assert.Equal(new DateOnly(2026, 2, 10), dienstwunschRepository.EntfernteWuensche[0].WunschDatum);

        Assert.DoesNotContain(
            dienstwunschRepository.Wuensche,
            x => x.UserId == userId &&
                 x.WunschDatum == new DateOnly(2026, 2, 10));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.UserId == andererUserId &&
                 x.WunschDatum == new DateOnly(2026, 2, 10));

        Assert.Contains(
            dienstwunschRepository.Wuensche,
            x => x.UserId == userId &&
                 x.WunschDatum == new DateOnly(2026, 2, 20));

        Assert.Equal(1, dienstwunschRepository.SaveChangesAufrufe);
    }

    [Fact]
    public async Task ZeitraumSpeichern_SpeichertUrlaubAuchOhneOffenePeriodenOhneWuenscheZuBereinigen()
    {
        // Arrange
        var userId = "mitarbeiter-1";
        var jahr = 2026;
        var von = new DateOnly(2026, 2, 10);
        var bis = new DateOnly(2026, 2, 12);

        var leseMitarbeiterService = ErzeugeReadItwMitarbeiterprofileService(userId);

        var urlaubsanspruchRepository = new FakeMitarbeiterUrlaubsanspruchRepository();
        var urlaubszeitraumRepository = new FakeMitarbeiterUrlaubszeitraumRepository();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            OffenePerioden = []
        };

        var irgendeinePeriodeId = Guid.NewGuid();

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(irgendeinePeriodeId, userId, new DateOnly(2026, 2, 10), DienstwunschTyp.Wunsch),
            ErzeugeWunsch(irgendeinePeriodeId, userId, new DateOnly(2026, 2, 11), DienstwunschTyp.NichtVerfuegbar)
            ]
        };

        var currentUserAccessor = ErzeugeGueltigenUrlaubsplanerWachleiterAccessor();

        var controller = new UrlaubsplanerController(
            leseMitarbeiterService,
            urlaubsanspruchRepository,
            urlaubszeitraumRepository,
            dienstplanPeriodeRepository,
            dienstwunschRepository,
            currentUserAccessor);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.TempData = new TempDataDictionary(
            controller.ControllerContext.HttpContext,
            new FakeTempDataProvider());

        // Act
        var result = await controller.ZeitraumSpeichern(
            userId,
            jahr,
            von,
            bis,
            "Urlaub",
            CancellationToken.None);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal(userId, redirectResult.RouteValues?["userId"]);
        Assert.Equal(jahr, redirectResult.RouteValues?["jahr"]);

        var successMessage = Assert.IsType<string>(controller.TempData["SuccessMessage"]);
        Assert.Equal("Der Urlaubszeitraum wurde hinterlegt.", successMessage);

        Assert.Single(urlaubszeitraumRepository.GespeicherteZeitraeume);

        Assert.Equal(2, dienstwunschRepository.Wuensche.Count);
        Assert.Empty(dienstwunschRepository.EntfernteWuensche);
        Assert.Equal(0, dienstwunschRepository.SaveChangesAufrufe);
    }

    private static ReadItwMitarbeiterprofileService ErzeugeReadItwMitarbeiterprofileService(string userId)
    {
        var bereichszuordnungRepository = new FakeBenutzerBereichszuordnungRepository
        {
            Zuordnungen =
            [
                new BenutzerBereichszuordnung(
                    Guid.NewGuid(),
                    userId,
                    Organisationsbereich.Intensivtransport,
                    Bereichsrolle.ItwMitarbeiter,
                    Fuehrungsverantwortung.Keine,
                    true,
                    DateTimeOffset.UtcNow)
            ]
        };

        var benutzerkontoRepository = new FakeBenutzerkontoRepository
        {
            Konten =
            [
                new BenutzerkontoDto(
                    userId,
                    "mitarbeiter1",
                    "mitarbeiter1@example.invalid",
                    false)
            ]
        };

        var allgemeinesProfil = new AllgemeinesMitarbeiterprofil(
            Guid.NewGuid(),
            userId,
            DateTimeOffset.UtcNow);

        allgemeinesProfil.AktualisiereStammdaten(
            "Max",
            "Mustermann",
            MitarbeiterBeschaeftigungsart.Festangestellt,
            null,
            null,
            null,
            null,
            null,
            DateTimeOffset.UtcNow);

        var itwProfil = new ItwMitarbeiterprofil(
            Guid.NewGuid(),
            userId,
            DateTimeOffset.UtcNow);

        return new ReadItwMitarbeiterprofileService(
            bereichszuordnungRepository,
            benutzerkontoRepository,
            new FakeItwMitarbeiterprofilRepository
            {
                Profile = [itwProfil]
            },
            new FakeAllgemeinesMitarbeiterprofilRepository
            {
                Profile = [allgemeinesProfil]
            });
    }

    private static DienstplanPeriode ErzeugePeriode(Guid periodeId, int jahr, int monat)
    {
        return new DienstplanPeriode(
            periodeId,
            jahr,
            monat,
            $"Periode {monat:D2}/{jahr}",
            wunschphaseIstOffen: true,
            erstelltVonUserId: "system",
            erstelltAm: DateTimeOffset.UtcNow);
    }

    private static Dienstwunsch ErzeugeWunsch(
        Guid periodeId,
        string userId,
        DateOnly datum,
        DienstwunschTyp wunschTyp)
    {
        return new Dienstwunsch(
            Guid.NewGuid(),
            periodeId,
            userId,
            datum,
            wunschTyp,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeCurrentUserContextAccessor : ICurrentUserContextAccessor
    {
        private readonly CurrentUserContextLookupResult _result;

        public FakeCurrentUserContextAccessor(CurrentUserContextLookupResult result)
        {
            _result = result;
        }

        public Task<CurrentUserContextLookupResult> GetCurrentAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(_result);
    }

    private sealed class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
            => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    private sealed class FakeMitarbeiterUrlaubsanspruchRepository : IMitarbeiterUrlaubsanspruchRepository
    {
        public Task<MitarbeiterUrlaubsanspruch?> GetAsync(
            string userId,
            int jahr,
            CancellationToken cancellationToken = default)
            => Task.FromResult<MitarbeiterUrlaubsanspruch?>(null);

        public Task<IReadOnlyList<MitarbeiterUrlaubsanspruch>> GetAlleFuerBenutzerAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MitarbeiterUrlaubsanspruch>>(Array.Empty<MitarbeiterUrlaubsanspruch>());

        public Task UpsertAsync(
            MitarbeiterUrlaubsanspruch anspruch,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeMitarbeiterUrlaubszeitraumRepository : IMitarbeiterUrlaubszeitraumRepository
    {
        public List<MitarbeiterUrlaubszeitraum> GespeicherteZeitraeume { get; } = [];

        public Task<MitarbeiterUrlaubszeitraum?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<MitarbeiterUrlaubszeitraum?>(null);

        public Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MitarbeiterUrlaubszeitraum>>(Array.Empty<MitarbeiterUrlaubszeitraum>());

        public Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerUndJahrAsync(
            string userId,
            int jahr,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MitarbeiterUrlaubszeitraum>>(Array.Empty<MitarbeiterUrlaubszeitraum>());

        public Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
            DateOnly datum,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

        public Task<bool> HatUeberschneidungAsync(
            string userId,
            DateOnly von,
            DateOnly bis,
            Guid? ausnahmeId = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddOrUpdateAsync(
            MitarbeiterUrlaubszeitraum zeitraum,
            CancellationToken cancellationToken = default)
        {
            GespeicherteZeitraeume.Add(zeitraum);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public IReadOnlyList<DienstplanPeriode> OffenePerioden { get; set; } = Array.Empty<DienstplanPeriode>();

        public Task<bool> ExistiertAsync(
            int jahr,
            int monat,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddAsync(
            DienstplanPeriode periode,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<DienstplanPeriode?> GetByIdAsync(
            Guid periodeId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<int> CountOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(OffenePerioden.Count);

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(OffenePerioden);

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstwunschRepository : IDienstwunschRepository
    {
        public List<Dienstwunsch> Wuensche { get; set; } = [];
        public List<Dienstwunsch> EntfernteWuensche { get; } = [];
        public int SaveChangesAufrufe { get; private set; }

        public Task<Dienstwunsch?> GetAsync(
            Guid dienstplanPeriodeId,
            string userId,
            DateOnly wunschDatum,
            CancellationToken cancellationToken = default)
        {
            var result = Wuensche.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase) &&
                x.WunschDatum == wunschDatum);

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerBenutzerAsync(
            Guid dienstplanPeriodeId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Wuensche
                .Where(x =>
                    x.DienstplanPeriodeId == dienstplanPeriodeId &&
                    string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Task.FromResult<IReadOnlyList<Dienstwunsch>>(result);
        }

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            var result = Wuensche
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray();

            return Task.FromResult<IReadOnlyList<Dienstwunsch>>(result);
        }

        public Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerTagAsync(
            Guid dienstplanPeriodeId,
            DateOnly wunschDatum,
            DienstwunschTyp? wunschTyp = null,
            CancellationToken cancellationToken = default)
        {
            var query = Wuensche.Where(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                x.WunschDatum == wunschDatum);

            if (wunschTyp.HasValue)
            {
                query = query.Where(x => x.WunschTyp == wunschTyp.Value);
            }

            return Task.FromResult<IReadOnlyList<Dienstwunsch>>(query.ToArray());
        }

        public Task AddAsync(
            Dienstwunsch dienstwunsch,
            CancellationToken cancellationToken = default)
        {
            Wuensche.Add(dienstwunsch);
            return Task.CompletedTask;
        }

        public void Remove(Dienstwunsch dienstwunsch)
        {
            EntfernteWuensche.Add(dienstwunsch);
            Wuensche.Remove(dienstwunsch);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAufrufe++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBenutzerBereichszuordnungRepository : IBenutzerBereichszuordnungRepository
    {
        public IReadOnlyList<BenutzerBereichszuordnung> Zuordnungen { get; set; }
            = Array.Empty<BenutzerBereichszuordnung>();

        public Task<BenutzerBereichszuordnung?> GetAktivePrimaereZuordnungAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Zuordnungen.FirstOrDefault(x =>
                x.IsActive &&
                x.IsPrimary &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<BenutzerBereichszuordnung>> GetAktivePrimaereZuordnungenByBereichAsync(
            Organisationsbereich bereich,
            CancellationToken cancellationToken = default)
        {
            var result = Zuordnungen
                .Where(x => x.IsActive && x.IsPrimary && x.Bereich == bereich)
                .ToArray();

            return Task.FromResult<IReadOnlyList<BenutzerBereichszuordnung>>(result);
        }

        public Task AddAsync(
            BenutzerBereichszuordnung zuordnung,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeBenutzerkontoRepository : IBenutzerkontoRepository
    {
        public IReadOnlyList<BenutzerkontoDto> Konten { get; set; } = Array.Empty<BenutzerkontoDto>();

        public Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Konten
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(result);
        }

        public Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<BenutzerkontoDto>>(Array.Empty<BenutzerkontoDto>());

        public Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
            string benutzername,
            string email,
            string passwort,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<UpdateBenutzerkontoStatusRepositoryResult> AktivierenAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SetTemporaeresPasswortRepositoryResult> SetzeTemporaeresPasswortAsync(
            string userId,
            string temporaeresPasswort,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task EntfernePasswortwechselPflichtAsync(
            string userId,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SynchronisiereNamensClaimsAsync(
            string userId,
            string? vorname,
            string? nachname,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeItwMitarbeiterprofilRepository : IItwMitarbeiterprofilRepository
    {
        public IReadOnlyList<ItwMitarbeiterprofil> Profile { get; set; } = Array.Empty<ItwMitarbeiterprofil>();

        public Task EnsureStandardqualifikationenAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<ItwQualifikation>> GetAktiveQualifikationenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ItwQualifikation>>(Array.Empty<ItwQualifikation>());

        public Task<IReadOnlyList<ItwMitarbeiterprofil>> GetByUserIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Profile
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<ItwMitarbeiterprofil>>(result);
        }

        public Task<ItwMitarbeiterprofil?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Profile.FirstOrDefault(x =>
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task UpsertQualifikationenAsync(
            string userId,
            Guid hauptqualifikationId,
            IReadOnlyCollection<Guid> zusatzqualifikationIds,
            DateTimeOffset aktualisiertAm,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeAllgemeinesMitarbeiterprofilRepository : IAllgemeinesMitarbeiterprofilRepository
    {
        public IReadOnlyList<AllgemeinesMitarbeiterprofil> Profile { get; set; }
            = Array.Empty<AllgemeinesMitarbeiterprofil>();

        public Task<AllgemeinesMitarbeiterprofil?> GetByUserIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Profile.FirstOrDefault(x =>
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<AllgemeinesMitarbeiterprofil>> GetByUserIdsAsync(
            IReadOnlyCollection<string> userIds,
            CancellationToken cancellationToken = default)
        {
            var idSet = userIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = Profile
                .Where(x => idSet.Contains(x.UserId))
                .ToArray();

            return Task.FromResult<IReadOnlyList<AllgemeinesMitarbeiterprofil>>(result);
        }

        public Task UpsertAsync(
            string userId,
            string vorname,
            string nachname,
            MitarbeiterBeschaeftigungsart beschaeftigungsart,
            string? telefonnummer,
            string? strasse,
            string? hausnummer,
            string? postleitzahl,
            string? ort,
            DateTimeOffset aktualisiertAm,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;


        
    }

    private static ICurrentUserContextAccessor ErzeugeGueltigenUrlaubsplanerWachleiterAccessor()
    {
        return new FakeCurrentUserContextAccessor(
            CurrentUserContextLookupResult.Erfolg(
                new CurrentUserContext(
                    "wachleiter-1",
                    OrganisationsbereichCode.Intensivtransport,
                    BereichsrolleCode.Wachleiter,
                    FuehrungsverantwortungCode.OperativeLeitung,
                    new[] { ModulCode.Dienstplan })));
    }
}