using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Personnel.Enums;
using ITW.Domain.Personnel.Qualifications;
using Xunit;

namespace ITW.Dienstplan.Test;

public sealed class ReadAutomatischePlanungsvorschauServiceTests
{
    [Fact]
    public async Task ExecuteAsync_GibtFehlerZurueck_WennPeriodeIdLeerIst()
    {
        // Arrange
        var service = ErzeugeService();

        // Act
        var result = await service.ExecuteAsync(Guid.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Die Dienstplanperiode ist ungültig.", result.ErrorMessage);
        Assert.Empty(result.Tage);
    }

    [Fact]
    public async Task ExecuteAsync_GibtFehlerZurueck_WennPeriodeNichtGefundenWird()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = null
        };

        var service = ErzeugeService(dienstplanPeriodeRepository: dienstplanPeriodeRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Die Dienstplanperiode wurde nicht gefunden.", result.ErrorMessage);
        Assert.Empty(result.Tage);
    }

    [Fact]
    public async Task ExecuteAsync_SetztFreelancerAlsPflichtfall_WennRestbedarfGenauVerbleibendenWunschtagenEntspricht()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var freelancerUserId = "arzt.freelancer";

        var ersterWunschtag = new DateOnly(2026, 2, 3);
        var zweiterWunschtag = new DateOnly(2026, 2, 4);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                    freelancerUserId,
                    "Dr. Freelancer",
                    MitarbeiterBeschaeftigungsart.Freelancer,
                    ItwQualifikationsCodes.Arzt,
                    "Arzt")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, freelancerUserId, ersterWunschtag, DienstwunschTyp.Wunsch),
                ErzeugeWunsch(periodeId, freelancerUserId, zweiterWunschtag, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, freelancerUserId, 2)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var ersterTag = Assert.Single(result.Tage.Where(x => x.Datum == ersterWunschtag));
        var zweiterTag = Assert.Single(result.Tage.Where(x => x.Datum == zweiterWunschtag));

        Assert.Equal(freelancerUserId, ersterTag.ArztUserId);
        Assert.Equal(freelancerUserId, zweiterTag.ArztUserId);

        var ersteArztEntscheidung = Assert.Single(
            ersterTag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));

        var zweiteArztEntscheidung = Assert.Single(
            zweiterTag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));

        Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, ersteArztEntscheidung.Art);
        Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, zweiteArztEntscheidung.Art);

        Assert.Contains("Pflichtfall", ersteArztEntscheidung.Nachricht);
        Assert.Contains("Pflichtfall", zweiteArztEntscheidung.Nachricht);
    }

    [Fact]
    public async Task ExecuteAsync_PlantFreelancerNichtUeberDieMonatsanzahlHinaus()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var freelancerUserId = "arzt.freelancer";

        var ersterWunschtag = new DateOnly(2026, 2, 3);
        var zweiterWunschtag = new DateOnly(2026, 2, 4);
        var dritterWunschtag = new DateOnly(2026, 2, 5);

        var alleWunschtage = new[] { ersterWunschtag, zweiterWunschtag, dritterWunschtag };

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                freelancerUserId,
                "Dr. Freelancer",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Arzt,
                "Arzt")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, freelancerUserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, freelancerUserId, zweiterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, freelancerUserId, dritterWunschtag, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, freelancerUserId, 2)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var zugewieseneArzttage = result.Tage
            .Where(x => string.Equals(x.ArztUserId, freelancerUserId, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Datum)
            .ToArray();

        Assert.Equal(2, zugewieseneArzttage.Length);
        Assert.All(zugewieseneArzttage, datum => Assert.Contains(datum, alleWunschtage));

        var betrachteteWunschtage = result.Tage
            .Where(x => alleWunschtage.Contains(x.Datum))
            .OrderBy(x => x.Datum)
            .ToArray();

        Assert.Equal(3, betrachteteWunschtage.Length);
        Assert.Equal(2, betrachteteWunschtage.Count(x => x.ArztUserId == freelancerUserId));
        Assert.Equal(1, betrachteteWunschtage.Count(x => string.IsNullOrWhiteSpace(x.ArztUserId)));
    }

    [Fact]
    public async Task ExecuteAsync_SetztDreiVerpflichtendeFreelancerAnGemeinsamenWunschtagenVollstaendigEin()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.freelancer";
        var nfs1UserId = "nfs1.freelancer";
        var nfs2UserId = "nfs2.freelancer";

        var ersterWunschtag = new DateOnly(2026, 2, 3);
        var zweiterWunschtag = new DateOnly(2026, 2, 4);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Dr. Freelancer",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                nfs1UserId,
                "NFS Eins",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs2UserId,
                "NFS Zwei",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, arztUserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, arztUserId, zweiterWunschtag, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfs1UserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfs1UserId, zweiterWunschtag, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfs2UserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfs2UserId, zweiterWunschtag, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, arztUserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfs1UserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfs2UserId, 2)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var ersterTag = Assert.Single(result.Tage.Where(x => x.Datum == ersterWunschtag));
        var zweiterTag = Assert.Single(result.Tage.Where(x => x.Datum == zweiterWunschtag));

        PruefeVollstaendigGesetztenFreelancerPflichttag(ersterTag, arztUserId, nfs1UserId, nfs2UserId);
        PruefeVollstaendigGesetztenFreelancerPflichttag(zweiterTag, arztUserId, nfs1UserId, nfs2UserId);
    }

    [Fact]
    public async Task ExecuteAsync_MeldetKonflikt_WennDreiVerpflichtendeNfsFreelancerAufZweiNfsSlotsTreffen()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.freelancer";
        var nfs1UserId = "nfs1.freelancer";
        var nfs2UserId = "nfs2.freelancer";
        var nfs3UserId = "nfs3.freelancer";

        var ersterWunschtag = new DateOnly(2026, 2, 3);
        var zweiterWunschtag = new DateOnly(2026, 2, 4);

        var erwarteteNfsUserIds = new[] { nfs1UserId, nfs2UserId, nfs3UserId };

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Dr. Freelancer",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                nfs1UserId,
                "NFS Eins",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs2UserId,
                "NFS Zwei",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs3UserId,
                "NFS Drei",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, arztUserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, arztUserId, zweiterWunschtag, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfs1UserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfs1UserId, zweiterWunschtag, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfs2UserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfs2UserId, zweiterWunschtag, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfs3UserId, ersterWunschtag, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfs3UserId, zweiterWunschtag, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, arztUserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfs1UserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfs2UserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfs3UserId, 2)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var ersterTag = Assert.Single(result.Tage.Where(x => x.Datum == ersterWunschtag));
        var zweiterTag = Assert.Single(result.Tage.Where(x => x.Datum == zweiterWunschtag));

        PruefeNfsPflichtfallKonflikttag(ersterTag, arztUserId, erwarteteNfsUserIds);
        PruefeNfsFolgetagNachPflichtfallKonflikt(zweiterTag, arztUserId, erwarteteNfsUserIds);
    }

    [Fact]
    public async Task ExecuteAsync_PlantHonorarkraftNurMitWunsch()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var honorarkraftMitWunschUserId = "honorar.mit.wunsch";
        var honorarkraftOhneWunschUserId = "honorar.ohne.wunsch";
        var datum = new DateOnly(2026, 2, 3);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                honorarkraftMitWunschUserId,
                "Honorarkraft Mit Wunsch",
                MitarbeiterBeschaeftigungsart.Honorarkraft,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                honorarkraftOhneWunschUserId,
                "Honorarkraft Ohne Wunsch",
                MitarbeiterBeschaeftigungsart.Honorarkraft,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, honorarkraftMitWunschUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        var gesetzteNfsUserIds = new[]
        {
        tag.Notfallsanitaeter1UserId,
        tag.Notfallsanitaeter2UserId
    }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Cast<string>()
        .ToArray();

        Assert.Contains(honorarkraftMitWunschUserId, gesetzteNfsUserIds);
        Assert.DoesNotContain(honorarkraftOhneWunschUserId, gesetzteNfsUserIds);

        var entscheidungDerHonorarkraft = Assert.Single(
            tag.SlotEntscheidungen.Where(x => x.UserId == honorarkraftMitWunschUserId));

        Assert.Equal(AutomatischePlanungZuweisungsArt.HonorarkraftWunsch, entscheidungDerHonorarkraft.Art);
        Assert.Equal("Wunsch der Honorarkraft wurde berücksichtigt.", entscheidungDerHonorarkraft.Nachricht);
    }

    [Fact]
    public async Task ExecuteAsync_MarkiertFreelancerKonfliktAmFruehestenRelevantenWunschtag()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.fest";
        var fillerNfsUserId = "nfs.fest";
        var nfsAUserId = "nfs.a";
        var nfsBUserId = "nfs.b";
        var nfsCUserId = "nfs.c";

        var tag1 = new DateOnly(2026, 2, 3);
        var tag2 = new DateOnly(2026, 2, 4);
        var tag3 = new DateOnly(2026, 2, 5);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Arzt Fest",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                fillerNfsUserId,
                "NFS Filler",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfsAUserId,
                "NFS A",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfsBUserId,
                "NFS B",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfsCUserId,
                "NFS C",
                MitarbeiterBeschaeftigungsart.Freelancer,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, nfsAUserId, tag1, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfsAUserId, tag2, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfsBUserId, tag1, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfsBUserId, tag2, DienstwunschTyp.Wunsch),

            ErzeugeWunsch(periodeId, nfsCUserId, tag1, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, nfsCUserId, tag3, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, nfsAUserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfsBUserId, 2),
            ErzeugeFreelancerMonatswunsch(periodeId, nfsCUserId, 2)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var ersterTag = Assert.Single(result.Tage.Where(x => x.Datum == tag1));
        var zweiterTag = Assert.Single(result.Tage.Where(x => x.Datum == tag2));
        var dritterTag = Assert.Single(result.Tage.Where(x => x.Datum == tag3));

        // Der nicht mehr sauber erreichbare Freelancer-Konflikt wird am frühesten relevanten Wunschtag markiert.
        Assert.Contains(
    ersterTag.Konflikte,
    x => x.Art == AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar &&
         x.Nachricht.Contains("NFS C", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            zweiterTag.Konflikte,
            x => x.Art == AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar);

        Assert.DoesNotContain(
            dritterTag.Konflikte,
            x => x.Art == AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar);

        // NFS C bekommt am Ende genau noch einen Dienst, weil nur noch Tag 3 übrig bleibt.
        var tageMitNfsC = result.Tage
            .Where(x =>
                string.Equals(x.Notfallsanitaeter1UserId, nfsCUserId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Notfallsanitaeter2UserId, nfsCUserId, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Datum)
            .ToArray();

        Assert.Single(tageMitNfsC);
        Assert.Equal(tag3, tageMitNfsC[0]);
    }

    [Fact]
    public async Task ExecuteAsync_BevorzugtFestangestelltenWunschblockVorEinzelwunsch()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var blockArztUserId = "arzt.block";
        var einzelArztUserId = "arzt.einzel";
        var nfs1UserId = "nfs.1";
        var nfs2UserId = "nfs.2";

        var tag1 = new DateOnly(2026, 2, 3);
        var tag2 = new DateOnly(2026, 2, 4);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                blockArztUserId,
                "Zeta Blockarzt",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                einzelArztUserId,
                "Alpha Einzelarzt",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                nfs1UserId,
                "NFS Eins",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs2UserId,
                "NFS Zwei",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, blockArztUserId, tag1, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, blockArztUserId, tag2, DienstwunschTyp.Wunsch),
            ErzeugeWunsch(periodeId, einzelArztUserId, tag1, DienstwunschTyp.Wunsch)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var ersterTag = Assert.Single(result.Tage.Where(x => x.Datum == tag1));
        var zweiterTag = Assert.Single(result.Tage.Where(x => x.Datum == tag2));

        Assert.Equal(blockArztUserId, ersterTag.ArztUserId);
        Assert.Equal(blockArztUserId, zweiterTag.ArztUserId);

        var ersteArztEntscheidung = Assert.Single(
            ersterTag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));

        Assert.Equal(AutomatischePlanungZuweisungsArt.MitarbeiterWunschblock, ersteArztEntscheidung.Art);
        Assert.Contains("Zusammenhängender Wunschblock", ersteArztEntscheidung.Nachricht, StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            ersterTag.SlotEntscheidungen,
            x => string.Equals(x.UserId, einzelArztUserId, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_SetztFestangestelltenEinzelwunschAlsMitarbeiterwunsch()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.einzel";
        var nfs1UserId = "nfs.1";
        var nfs2UserId = "nfs.2";
        var datum = new DateOnly(2026, 2, 3);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Einzelarzt",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                nfs1UserId,
                "NFS Eins",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs2UserId,
                "NFS Zwei",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, arztUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(arztUserId, tag.ArztUserId);

        var arztEntscheidung = Assert.Single(
            tag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));

        Assert.Equal(AutomatischePlanungZuweisungsArt.MitarbeiterWunsch, arztEntscheidung.Art);
        Assert.Equal("Mitarbeiterwunsch wurde berücksichtigt.", arztEntscheidung.Nachricht);
    }

    [Fact]
    public async Task ExecuteAsync_EnthaeltKeineWochenendtageInDerVorschau()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var montag = new DateOnly(2026, 2, 2);
        var samstag = new DateOnly(2026, 2, 7);
        var sonntag = new DateOnly(2026, 2, 8);

        Assert.Equal(DayOfWeek.Monday, montag.DayOfWeek);
        Assert.Equal(DayOfWeek.Saturday, samstag.DayOfWeek);
        Assert.Equal(DayOfWeek.Sunday, sonntag.DayOfWeek);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.Contains(result.Tage, x => x.Datum == montag);
        Assert.DoesNotContain(result.Tage, x => x.Datum == samstag);
        Assert.DoesNotContain(result.Tage, x => x.Datum == sonntag);
    }

    [Fact]
    public async Task ExecuteAsync_EnthaeltKeineFeiertageInDerVorschau()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var feiertag = new DateOnly(2026, 1, 1);
        var normalerWerktag = new DateOnly(2026, 1, 2);

        Assert.Equal(DayOfWeek.Thursday, feiertag.DayOfWeek);
        Assert.Equal(DayOfWeek.Friday, normalerWerktag.DayOfWeek);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 1)
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        Assert.DoesNotContain(result.Tage, x => x.Datum == feiertag);
        Assert.Contains(result.Tage, x => x.Datum == normalerWerktag);
    }

    [Fact]
    public async Task ExecuteAsync_PlantMitarbeiterMitUrlaubNichtAutomatischEin()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var urlaubArztUserId = "arzt.urlaub";
        var verfuegbarArztUserId = "arzt.verfuegbar";
        var nfs1UserId = "nfs.1";
        var nfs2UserId = "nfs.2";
        var datum = new DateOnly(2026, 2, 3);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                urlaubArztUserId,
                "Arzt Urlaub",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                verfuegbarArztUserId,
                "Arzt Verfügbar",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                nfs1UserId,
                "NFS Eins",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                nfs2UserId,
                "NFS Zwei",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, urlaubArztUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var urlaubsRepository = new FakeDienstplanUrlaubszeitraumRepository();
        urlaubsRepository.UserIdsProDatum[datum] = new[] { urlaubArztUserId };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            dienstplanUrlaubszeitraumRepository: urlaubsRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(verfuegbarArztUserId, tag.ArztUserId);
        Assert.NotEqual(urlaubArztUserId, tag.ArztUserId);

        var arztEntscheidung = Assert.Single(
            tag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));

        Assert.Equal(verfuegbarArztUserId, arztEntscheidung.UserId);
        Assert.Equal(AutomatischePlanungZuweisungsArt.MitarbeiterLueckenfueller, arztEntscheidung.Art);
        Assert.Equal(
            "Als Lückenfüller gesetzt, weil kein höher priorisierter Wunschkandidat verfügbar war.",
            arztEntscheidung.Nachricht);

        Assert.DoesNotContain(
            tag.SlotEntscheidungen,
            x => string.Equals(x.UserId, urlaubArztUserId, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_BevorzugtBeimZweitenNfsSlotDurchGelerntenTeambonusEinenAnderenKandidatenBeiGleicherFachlage()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.fix";
        var alphaNfsUserId = "nfs.alpha";
        var betaNfsUserId = "nfs.beta";
        var zetaNfsUserId = "nfs.zeta";

        var datum = new DateOnly(2026, 2, 3);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Arzt Fix",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                alphaNfsUserId,
                "Alpha NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                betaNfsUserId,
                "Beta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                zetaNfsUserId,
                "Zeta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Alpha soll fachlich sauber den ersten NFS-Slot bekommen.
                ErzeugeWunsch(periodeId, alphaNfsUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var lernRepository = new FakeAutoplanLernereignisRepository
        {
            GrundbesetzungsLernereignisse =
            [
                ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2026, 1, 10),
                erfasstAm: new DateTimeOffset(2026, 1, 10, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2026, 1, 15),
                erfasstAm: new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            autoplanLernereignisRepository: lernRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(arztUserId, tag.ArztUserId);

        // Alpha bekommt NFS 1 über den Mitarbeiterwunsch.
        Assert.Equal(alphaNfsUserId, tag.Notfallsanitaeter1UserId);

        // Beim zweiten NFS-Slot sind Beta und Zeta fachlich gleich.
        // Durch den gelernten Teamkontext mit Arzt Fix + Alpha NFS soll Zeta gewählt werden.
        Assert.Equal(zetaNfsUserId, tag.Notfallsanitaeter2UserId);

        var nfs2Entscheidung = tag.SlotEntscheidungen.Single(x => x.SlotBezeichnung == "NFS 2");
    }

    [Fact]
    public async Task ExecuteAsync_GibtOhnePassendenTeamkontextKeinenLernbonusUndLaesstDieNormaleFachlogikGewinnen()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.fix";
        var alphaNfsUserId = "nfs.alpha";
        var betaNfsUserId = "nfs.beta";
        var zetaNfsUserId = "nfs.zeta";

        var datum = new DateOnly(2026, 2, 3);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Arzt Fix",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                alphaNfsUserId,
                "Alpha NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                betaNfsUserId,
                "Beta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                zetaNfsUserId,
                "Zeta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                // Alpha soll NFS 1 bekommen.
                ErzeugeWunsch(periodeId, alphaNfsUserId, datum, DienstwunschTyp.Wunsch),

            // Beta soll durch normale Fachlogik NFS 2 bekommen.
            ErzeugeWunsch(periodeId, betaNfsUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var lernRepository = new FakeAutoplanLernereignisRepository
        {
            GrundbesetzungsLernereignisse =
            [
                // Zeta hat gelernte Treffer, aber mit NICHT passendem Kontext.
                ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: "arzt.anders",
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2026, 1, 10),
                erfasstAm: new DateTimeOffset(2026, 1, 10, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: "arzt.anders",
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2026, 1, 15),
                erfasstAm: new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            autoplanLernereignisRepository: lernRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(arztUserId, tag.ArztUserId);
        Assert.Equal(alphaNfsUserId, tag.Notfallsanitaeter1UserId);

        // Trotz gelernter Zeta-Historie darf ohne passenden Kontext kein Lernbonus greifen.
        // Deshalb soll hier die normale Fachlogik ("Mitarbeiterwunsch") Beta auf NFS 2 setzen.
        Assert.Equal(betaNfsUserId, tag.Notfallsanitaeter2UserId);
        Assert.NotEqual(zetaNfsUserId, tag.Notfallsanitaeter2UserId);

        var nfs2Entscheidung = tag.SlotEntscheidungen.Single(x => x.SlotBezeichnung == "NFS 2");

        Assert.Equal(betaNfsUserId, nfs2Entscheidung.UserId);
        Assert.Equal(AutomatischePlanungZuweisungsArt.MitarbeiterWunsch, nfs2Entscheidung.Art);
        Assert.DoesNotContain(
            "Lernbonus aus ähnlicher manuell bestätigter Teamkonstellation",
            nfs2Entscheidung.Nachricht,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_BeruecksichtigtNegativesKorrekturlernenBeiDerKandidatenauswahl()
    {
        // Arrange
        var periodeId = Guid.NewGuid();

        var arztUserId = "arzt.fix";
        var alphaNfsUserId = "nfs.alpha";
        var betaNfsUserId = "nfs.beta";
        var zetaNfsUserId = "nfs.zeta";

        var datum = new DateOnly(2026, 4, 1);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 4)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                arztUserId,
                "Arzt Fix",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Arzt,
                "Arzt"),
            ErzeugeMitarbeiter(
                alphaNfsUserId,
                "Alpha NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                betaNfsUserId,
                "Beta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter"),
            ErzeugeMitarbeiter(
                zetaNfsUserId,
                "Zeta NFS",
                MitarbeiterBeschaeftigungsart.Festangestellt,
                ItwQualifikationsCodes.Notfallsanitaeter,
                "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, alphaNfsUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var lernRepository = new FakeAutoplanLernereignisRepository
        {
            GrundbesetzungsLernereignisse =
            [
                ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2025, 8, 1),
                erfasstAm: new DateTimeOffset(2025, 8, 1, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsLernereignis(
                neueUserId: zetaNfsUserId,
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: zetaNfsUserId,
                datum: new DateOnly(2025, 8, 10),
                erfasstAm: new DateTimeOffset(2025, 8, 10, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsKorrekturLernereignis(
                vorherigeUserId: zetaNfsUserId,
                neueUserId: "nfs.omega",
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: "nfs.omega",
                datum: new DateOnly(2026, 3, 20),
                erfasstAm: new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero)),
            ErzeugeGrundbesetzungsKorrekturLernereignis(
                vorherigeUserId: zetaNfsUserId,
                neueUserId: "nfs.omega",
                besetzungsSlotCode: DienstbesetzungsSlotCode.Notfallsanitaeter2,
                kontextArztUserId: arztUserId,
                kontextNotfallsanitaeter1UserId: alphaNfsUserId,
                kontextNotfallsanitaeter2UserId: "nfs.omega",
                datum: new DateOnly(2026, 3, 25),
                erfasstAm: new DateTimeOffset(2026, 3, 25, 8, 0, 0, TimeSpan.Zero))
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            autoplanLernereignisRepository: lernRepository);

        // Act
        var result = await service.ExecuteAsync(periodeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(arztUserId, tag.ArztUserId);
        Assert.Equal(alphaNfsUserId, tag.Notfallsanitaeter1UserId);

        // Zeta hat zwar alte positive Teamtreffer, wurde im gleichen Kontext aber zuletzt wiederholt ersetzt.
        // Deshalb soll das negative Korrekturlernen greifen und Beta auf NFS 2 setzen.
        Assert.Equal(betaNfsUserId, tag.Notfallsanitaeter2UserId);
        Assert.NotEqual(zetaNfsUserId, tag.Notfallsanitaeter2UserId);

        var nfs2Entscheidung = tag.SlotEntscheidungen.Single(x => x.SlotBezeichnung == "NFS 2");

        Assert.Equal(betaNfsUserId, nfs2Entscheidung.UserId);
        Assert.DoesNotContain(
            "Lernbonus aus ähnlicher manuell bestätigter Teamkonstellation",
            nfs2Entscheidung.Nachricht,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_BlendetAbstrakteAutoplanKonflikteAus_WennDerTagBereitsManuellVollstaendigGeloestIst()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var datum = new DateOnly(2026, 2, 3);

        var freelancerArztUserId = "arzt.freelancer";
        var manuellerArztUserId = "arzt.manuell";
        var nfs1UserId = "nfs.eins";
        var nfs2UserId = "nfs.zwei";

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 2)
        };

        var mitarbeiterRepository = new FakeDienstplanMitarbeiterPlanungsRepository
        {
            Mitarbeiter =
            [
                ErzeugeMitarbeiter(
                    freelancerArztUserId,
                    "Dr. Freelancer",
                    MitarbeiterBeschaeftigungsart.Freelancer,
                    ItwQualifikationsCodes.Arzt,
                    "Arzt"),
                ErzeugeMitarbeiter(
                    manuellerArztUserId,
                    "Dr. Manuell",
                    MitarbeiterBeschaeftigungsart.Festangestellt,
                    ItwQualifikationsCodes.Arzt,
                    "Arzt"),
                ErzeugeMitarbeiter(
                    nfs1UserId,
                    "NFS Eins",
                    MitarbeiterBeschaeftigungsart.Festangestellt,
                    ItwQualifikationsCodes.Notfallsanitaeter,
                    "Notfallsanitäter"),
                ErzeugeMitarbeiter(
                    nfs2UserId,
                    "NFS Zwei",
                    MitarbeiterBeschaeftigungsart.Festangestellt,
                    ItwQualifikationsCodes.Notfallsanitaeter,
                    "Notfallsanitäter")
            ]
        };

        var dienstwunschRepository = new FakeDienstwunschRepository
        {
            Wuensche =
            [
                ErzeugeWunsch(periodeId, freelancerArztUserId, datum, DienstwunschTyp.Wunsch)
            ]
        };

        var freelancerMonatswunschRepository = new FakeFreelancerMonatswunschRepository
        {
            Eintraege =
            [
                ErzeugeFreelancerMonatswunsch(periodeId, freelancerArztUserId, 1)
            ]
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege =
            [
                new GeplanterDienstTag(
                    Guid.NewGuid(),
                    periodeId,
                    datum,
                    manuellerArztUserId,
                    nfs1UserId,
                    nfs2UserId,
                    "wachleiter-1",
                    DateTimeOffset.UtcNow)
            ]
        };

        var autoplanLernereignisRepository = new FakeAutoplanLernereignisRepository
        {
            GrundbesetzungsLernereignisse =
            [
                new AutoplanLernereignis(
                    Guid.NewGuid(),
                    periodeId,
                    datum,
                    DienstbesetzungsSlotCode.Arzt,
                    AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
                    vorherigeUserId: freelancerArztUserId,
                    neueUserId: manuellerArztUserId,
                    urspruenglichGeplanterUserId: null,
                    kontextArztUserId: manuellerArztUserId,
                    kontextNotfallsanitaeter1UserId: nfs1UserId,
                    kontextNotfallsanitaeter2UserId: nfs2UserId,
                    ausfallGrundCode: null,
                    bearbeitetVonUserId: "wachleiter-1",
                    erfasstAm: DateTimeOffset.UtcNow)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository: mitarbeiterRepository,
            dienstwunschRepository: dienstwunschRepository,
            freelancerMonatswunschRepository: freelancerMonatswunschRepository,
            geplanterDienstTagRepository: geplanterDienstTagRepository,
            autoplanLernereignisRepository: autoplanLernereignisRepository);

        // Act
        var result = await service.ExecuteAsync(
            periodeId,
            AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal(manuellerArztUserId, tag.ArztUserId);
        Assert.Equal(nfs1UserId, tag.Notfallsanitaeter1UserId);
        Assert.Equal(nfs2UserId, tag.Notfallsanitaeter2UserId);
        Assert.False(tag.HatKonflikt);
        Assert.Empty(tag.Konflikte);
    }

    [Fact]
    public async Task ExecuteAsync_RespektiertManuellBearbeitetenTagAuchBeiKompletterNeuberechnung()
    {
        // Arrange
        var periodeId = Guid.NewGuid();
        var datum = new DateOnly(2026, 4, 17);

        var dienstplanPeriodeRepository = new FakeDienstplanPeriodeRepository
        {
            PeriodeById = ErzeugePeriode(periodeId, 2026, 4)
        };

        var geplanterDienstTagRepository = new FakeGeplanterDienstTagRepository
        {
            Eintraege =
            [
                new GeplanterDienstTag(
                Guid.NewGuid(),
                periodeId,
                datum,
                arztUserId: "arzt.manuell",
                notfallsanitaeter1UserId: "nfs.manuell.1",
                notfallsanitaeter2UserId: "nfs.manuell.2",
                aktualisiertVonUserId: "wachleiter-1",
                aktualisiertAm: DateTimeOffset.UtcNow)
            ]
        };

        var autoplanLernereignisRepository = new FakeAutoplanLernereignisRepository
        {
            GrundbesetzungsLernereignisse =
            [
                new AutoplanLernereignis(
                Guid.NewGuid(),
                periodeId,
                datum,
                DienstbesetzungsSlotCode.Arzt,
                AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
                vorherigeUserId: "arzt.alt",
                neueUserId: "arzt.manuell",
                urspruenglichGeplanterUserId: null,
                kontextArztUserId: "arzt.manuell",
                kontextNotfallsanitaeter1UserId: "nfs.manuell.1",
                kontextNotfallsanitaeter2UserId: "nfs.manuell.2",
                ausfallGrundCode: null,
                bearbeitetVonUserId: "wachleiter-1",
                erfasstAm: DateTimeOffset.UtcNow)
            ]
        };

        var service = ErzeugeService(
            dienstplanPeriodeRepository: dienstplanPeriodeRepository,
            geplanterDienstTagRepository: geplanterDienstTagRepository,
            autoplanLernereignisRepository: autoplanLernereignisRepository);

        // Act
        var result = await service.ExecuteAsync(
            periodeId,
            AutomatischePlanungAusfuehrungsmodus.KomplettePeriodeNeuBerechnen);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        var tag = Assert.Single(result.Tage.Where(x => x.Datum == datum));

        Assert.Equal("arzt.manuell", tag.ArztUserId);
        Assert.Equal("nfs.manuell.1", tag.Notfallsanitaeter1UserId);
        Assert.Equal("nfs.manuell.2", tag.Notfallsanitaeter2UserId);

        Assert.False(tag.HatKonflikt);
        Assert.Empty(tag.Konflikte);
    }

    private static void PruefeNfsFolgetagNachPflichtfallKonflikt(
    AutomatischePlanungsvorschauTag tag,
    string erwarteterArztUserId,
    IReadOnlyCollection<string> erwarteteNfsUserIds)
    {
        Assert.False(tag.HatKonflikt);
        Assert.Equal(erwarteterArztUserId, tag.ArztUserId);

        var gesetzteNfsUserIds = new[]
        {
        tag.Notfallsanitaeter1UserId,
        tag.Notfallsanitaeter2UserId
    }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Cast<string>()
        .ToArray();

        Assert.Equal(2, gesetzteNfsUserIds.Length);
        Assert.Equal(2, gesetzteNfsUserIds.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.All(
            gesetzteNfsUserIds,
            userId => Assert.Contains(userId, erwarteteNfsUserIds, StringComparer.OrdinalIgnoreCase));

        var nfsEntscheidungen = tag.SlotEntscheidungen
            .Where(x => x.SlotBezeichnung is "NFS 1" or "NFS 2")
            .ToArray();

        Assert.Equal(2, nfsEntscheidungen.Length);
        Assert.All(
            nfsEntscheidungen,
            x => Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, x.Art));
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

    private static DienstplanMitarbeiterPlanungsstammdaten ErzeugeMitarbeiter(
        string userId,
        string anzeigeName,
        MitarbeiterBeschaeftigungsart beschaeftigungsart,
        string hauptqualifikationCode,
        string hauptqualifikationBezeichnung)
    {
        return new DienstplanMitarbeiterPlanungsstammdaten
        {
            UserId = userId,
            AnzeigeName = anzeigeName,
            Beschaeftigungsart = beschaeftigungsart,
            HauptqualifikationCode = hauptqualifikationCode,
            HauptqualifikationBezeichnung = hauptqualifikationBezeichnung,
            IstGesperrt = false,
            HatStammdatenprofil = true,
            HatItwProfil = true
        };
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

    private static FreelancerMonatswunsch ErzeugeFreelancerMonatswunsch(
        Guid periodeId,
        string userId,
        int gewuenschteDienste)
    {
        return new FreelancerMonatswunsch(
            Guid.NewGuid(),
            periodeId,
            userId,
            gewuenschteDienste,
            DateTimeOffset.UtcNow);
    }

    private static AutoplanLernereignis ErzeugeGrundbesetzungsLernereignis(
    string neueUserId,
    DienstbesetzungsSlotCode besetzungsSlotCode,
    string? kontextArztUserId,
    string? kontextNotfallsanitaeter1UserId,
    string? kontextNotfallsanitaeter2UserId,
    DateOnly datum,
    DateTimeOffset erfasstAm)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            besetzungsSlotCode,
            AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
            vorherigeUserId: "vorher.alt",
            neueUserId: neueUserId,
            urspruenglichGeplanterUserId: null,
            kontextArztUserId: kontextArztUserId,
            kontextNotfallsanitaeter1UserId: kontextNotfallsanitaeter1UserId,
            kontextNotfallsanitaeter2UserId: kontextNotfallsanitaeter2UserId,
            ausfallGrundCode: null,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: erfasstAm);
    }

    private static ReadAutomatischePlanungsvorschauService ErzeugeService(
    IDienstplanPeriodeRepository? dienstplanPeriodeRepository = null,
    IDienstplanMitarbeiterPlanungsRepository? dienstplanMitarbeiterPlanungsRepository = null,
    IDienstwunschRepository? dienstwunschRepository = null,
    IFreelancerMonatswunschRepository? freelancerMonatswunschRepository = null,
    IGeplanterDienstTagRepository? geplanterDienstTagRepository = null,
    IDienstplanUrlaubszeitraumRepository? dienstplanUrlaubszeitraumRepository = null,
    IAutoplanLernereignisRepository? autoplanLernereignisRepository = null)
    {
        return new ReadAutomatischePlanungsvorschauService(
            dienstplanPeriodeRepository ?? new FakeDienstplanPeriodeRepository(),
            dienstplanMitarbeiterPlanungsRepository ?? new FakeDienstplanMitarbeiterPlanungsRepository(),
            dienstwunschRepository ?? new FakeDienstwunschRepository(),
            freelancerMonatswunschRepository ?? new FakeFreelancerMonatswunschRepository(),
            geplanterDienstTagRepository ?? new FakeGeplanterDienstTagRepository(),
            dienstplanUrlaubszeitraumRepository ?? new FakeDienstplanUrlaubszeitraumRepository(),
            autoplanLernereignisRepository);
    }

    private sealed class FakeDienstplanPeriodeRepository : IDienstplanPeriodeRepository
    {
        public DienstplanPeriode? PeriodeById { get; set; }

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
            => Task.FromResult(PeriodeById);

        public Task<DienstplanPeriode?> GetAktuelleOffeneAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<DienstplanPeriode?>(null);

        public Task<int> CountOffeneAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> CountOffeneFuerBenutzerOhneWuenscheAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DienstplanPeriode>>(Array.Empty<DienstplanPeriode>());

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstplanMitarbeiterPlanungsRepository : IDienstplanMitarbeiterPlanungsRepository
    {
        public IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten> Mitarbeiter { get; set; }
            = Array.Empty<DienstplanMitarbeiterPlanungsstammdaten>();

        public Task<IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten>> GetAktivePlanungsmitarbeiterAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Mitarbeiter);
    }

    private sealed class FakeDienstwunschRepository : IDienstwunschRepository
    {
        public IReadOnlyList<Dienstwunsch> Wuensche { get; set; } = Array.Empty<Dienstwunsch>();

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
            => Task.CompletedTask;

        public void Remove(Dienstwunsch dienstwunsch)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeFreelancerMonatswunschRepository : IFreelancerMonatswunschRepository
    {
        public IReadOnlyList<FreelancerMonatswunsch> Eintraege { get; set; }
            = Array.Empty<FreelancerMonatswunsch>();

        public Task<FreelancerMonatswunsch?> GetAsync(
            Guid dienstplanPeriodeId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<FreelancerMonatswunsch>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray();

            return Task.FromResult<IReadOnlyList<FreelancerMonatswunsch>>(result);
        }

        public Task AddAsync(
            FreelancerMonatswunsch eintrag,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGeplanterDienstTagRepository : IGeplanterDienstTagRepository
    {
        public IReadOnlyList<GeplanterDienstTag> Eintraege { get; set; }
            = Array.Empty<GeplanterDienstTag>();

        public Task<GeplanterDienstTag?> GetAsync(
            Guid dienstplanPeriodeId,
            DateOnly dienstDatum,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege.FirstOrDefault(x =>
                x.DienstplanPeriodeId == dienstplanPeriodeId &&
                x.DienstDatum == dienstDatum);

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(
            Guid dienstplanPeriodeId,
            CancellationToken cancellationToken = default)
        {
            var result = Eintraege
                .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
                .ToArray();

            return Task.FromResult<IReadOnlyList<GeplanterDienstTag>>(result);
        }

        public Task AddAsync(
            GeplanterDienstTag geplanterDienstTag,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(GeplanterDienstTag geplanterDienstTag)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeDienstplanUrlaubszeitraumRepository : IDienstplanUrlaubszeitraumRepository
    {
        public Dictionary<DateOnly, IReadOnlyList<string>> UserIdsProDatum { get; } = new();

        public Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
            DateOnly datum,
            CancellationToken cancellationToken = default)
        {
            if (UserIdsProDatum.TryGetValue(datum, out var userIds))
            {
                return Task.FromResult(userIds);
            }

            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }
    }

    private sealed class FakeAutoplanLernereignisRepository : IAutoplanLernereignisRepository
    {
        public IReadOnlyList<AutoplanLernereignis> VertretungsLernereignisse { get; set; }
            = Array.Empty<AutoplanLernereignis>();

        public IReadOnlyList<AutoplanLernereignis> GrundbesetzungsLernereignisse { get; set; }
            = Array.Empty<AutoplanLernereignis>();

        public Task AddAsync(
            AutoplanLernereignis lernereignis,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(VertretungsLernereignisse);

        public Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(GrundbesetzungsLernereignisse);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private static void PruefeVollstaendigGesetztenFreelancerPflichttag(
    AutomatischePlanungsvorschauTag tag,
    string erwarteterArztUserId,
    string erwarteterNfs1UserId,
    string erwarteterNfs2UserId)
    {
        Assert.False(tag.HatKonflikt);
        Assert.True(string.IsNullOrWhiteSpace(tag.KonfliktText));

        Assert.Equal(erwarteterArztUserId, tag.ArztUserId);

        var gesetzteNfsUserIds = new[]
        {
        tag.Notfallsanitaeter1UserId,
        tag.Notfallsanitaeter2UserId
    };

        Assert.Contains(erwarteterNfs1UserId, gesetzteNfsUserIds);
        Assert.Contains(erwarteterNfs2UserId, gesetzteNfsUserIds);

        var arztEntscheidung = Assert.Single(tag.SlotEntscheidungen.Where(x => x.SlotBezeichnung == "Arzt"));
        Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, arztEntscheidung.Art);

        var nfsEntscheidungen = tag.SlotEntscheidungen
            .Where(x => x.SlotBezeichnung is "NFS 1" or "NFS 2")
            .OrderBy(x => x.SlotBezeichnung)
            .ToArray();

        Assert.Equal(2, nfsEntscheidungen.Length);
        Assert.All(nfsEntscheidungen, x =>
            Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, x.Art));
    }

    private static void PruefeNfsPflichtfallKonflikttag(
    AutomatischePlanungsvorschauTag tag,
    string erwarteterArztUserId,
    IReadOnlyCollection<string> erwarteteNfsUserIds)
    {
        Assert.True(tag.HatKonflikt);
        Assert.Contains(
            tag.Konflikte,
            x => x.Art == AutomatischePlanungKonfliktArt.MehrPflichtfaelleAlsFreieSlots);

        Assert.Equal(erwarteterArztUserId, tag.ArztUserId);

        var gesetzteNfsUserIds = new[]
        {
        tag.Notfallsanitaeter1UserId,
        tag.Notfallsanitaeter2UserId
    }
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Cast<string>()
        .ToArray();

        Assert.Equal(2, gesetzteNfsUserIds.Length);
        Assert.Equal(2, gesetzteNfsUserIds.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.All(
            gesetzteNfsUserIds,
            userId => Assert.Contains(userId, erwarteteNfsUserIds, StringComparer.OrdinalIgnoreCase));

        var nfsEntscheidungen = tag.SlotEntscheidungen
            .Where(x => x.SlotBezeichnung is "NFS 1" or "NFS 2")
            .ToArray();

        Assert.Equal(2, nfsEntscheidungen.Length);
        Assert.All(
            nfsEntscheidungen,
            x => Assert.Equal(AutomatischePlanungZuweisungsArt.PflichtfallFreelancer, x.Art));
    }

    private static AutoplanLernereignis ErzeugeGrundbesetzungsKorrekturLernereignis(
    string vorherigeUserId,
    string neueUserId,
    DienstbesetzungsSlotCode besetzungsSlotCode,
    string? kontextArztUserId,
    string? kontextNotfallsanitaeter1UserId,
    string? kontextNotfallsanitaeter2UserId,
    DateOnly datum,
    DateTimeOffset erfasstAm)
    {
        return new AutoplanLernereignis(
            Guid.NewGuid(),
            Guid.NewGuid(),
            datum,
            besetzungsSlotCode,
            AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
            vorherigeUserId: vorherigeUserId,
            neueUserId: neueUserId,
            urspruenglichGeplanterUserId: null,
            kontextArztUserId: kontextArztUserId,
            kontextNotfallsanitaeter1UserId: kontextNotfallsanitaeter1UserId,
            kontextNotfallsanitaeter2UserId: kontextNotfallsanitaeter2UserId,
            ausfallGrundCode: null,
            bearbeitetVonUserId: "wachleiter-1",
            erfasstAm: erfasstAm);
    }
}