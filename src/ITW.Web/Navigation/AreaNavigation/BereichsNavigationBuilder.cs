using ITW.Application.Organisation.Contracts;
using ITW.Application.Organisation.VisibilityScopes;
using ITW.Web.Security.CurrentUser;
using ITW.Web.ViewModels.Navigation;

namespace ITW.Web.Navigation.AreaNavigation;

public static class BereichsNavigationBuilder
{
    private static readonly BenutzerSichtbarkeitsScopeErmittler _benutzerScopeErmittler = new();

    public static BereichsNavigationViewModel Build(
        OrganisationsbereichCode navigationsBereich,
        CurrentUserContext? currentUser,
        string? aktuellerController,
        string? aktuelleAction,
        bool darfMeinUrlaub = true)
    {
        if (currentUser is null || currentUser.Bereich != navigationsBereich)
        {
            return new BereichsNavigationViewModel();
        }

        var sektionen = new List<BereichsNavigationSectionViewModel>
        {
            BaueDashboardSektion(navigationsBereich, aktuellerController, aktuelleAction)
        };

        var systemsteuerungSektion = BaueSystemsteuerungSektionWennErlaubt(
            navigationsBereich,
            currentUser,
            aktuellerController,
            aktuelleAction);

        if (systemsteuerungSektion is not null)
        {
            sektionen.Add(systemsteuerungSektion);
        }

        var dienstplanSektion = BaueDienstplanSektionWennErlaubt(
            navigationsBereich,
            currentUser,
            aktuellerController,
            aktuelleAction,
            darfMeinUrlaub);

        if (dienstplanSektion is not null)
        {
            sektionen.Add(dienstplanSektion);
        }

        var personalSektion = BauePersonalSektionWennErlaubt(
     navigationsBereich,
     currentUser,
     aktuellerController,
     aktuelleAction);

        if (personalSektion is not null)
        {
            sektionen.Add(personalSektion);
        }

        var fahrzeugmanagementSektion = BaueFahrzeugmanagementSektionWennErlaubt(
            navigationsBereich,
            currentUser,
            aktuellerController,
            aktuelleAction);

        if (fahrzeugmanagementSektion is not null)
        {
            sektionen.Add(fahrzeugmanagementSektion);
        }

        var lagerSektion = BaueLagermanagementSektionWennErlaubt(
            navigationsBereich,
            currentUser,
            aktuellerController,
            aktuelleAction);

        if (lagerSektion is not null)
        {
            sektionen.Add(lagerSektion);
        }

        return new BereichsNavigationViewModel
        {
            Sektionen = sektionen
        };
    }

    private static BereichsNavigationSectionViewModel BaueDashboardSektion(
        OrganisationsbereichCode navigationsBereich,
        string? aktuellerController,
        string? aktuelleAction)
    {
        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;

        return new BereichsNavigationSectionViewModel
        {
            Titel = "Übersicht",
            Eintraege = new[]
            {
                new BereichsNavigationItemViewModel
                {
                    Text = "Dashboard",
                    IconCssClass = ErmittleDashboardIcon(navigationsBereich),
                    Area = areaName,
                    Controller = "Dashboard",
                    Action = "Index",
                    IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Dashboard", "Index")
                }
            }
        };
    }

    private static BereichsNavigationSectionViewModel? BaueSystemsteuerungSektionWennErlaubt(
        OrganisationsbereichCode navigationsBereich,
        CurrentUserContext currentUser,
        string? aktuellerController,
        string? aktuelleAction)
    {
        if (navigationsBereich != OrganisationsbereichCode.Vorstand ||
            currentUser.Rolle != BereichsrolleCode.Vorstand)
        {
            return null;
        }

        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;

        return new BereichsNavigationSectionViewModel
        {
            Titel = "Systemsteuerung",
            Eintraege = new[]
            {
                new BereichsNavigationItemViewModel
                {
                    Text = "Module",
                    IconCssClass = "bi bi-diagram-3",
                    Area = areaName,
                    Controller = "Module",
                    Action = "Index",
                    IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Module", "Index")
                }
            }
        };
    }

    private static BereichsNavigationSectionViewModel? BaueDienstplanSektionWennErlaubt(
        OrganisationsbereichCode navigationsBereich,
        CurrentUserContext currentUser,
        string? aktuellerController,
        string? aktuelleAction,
        bool darfMeinUrlaub = true)
    {
        if (navigationsBereich != OrganisationsbereichCode.Intensivtransport)
        {
            return null;
        }

        if (!currentUser.HatModul(ModulCode.Dienstplan))
        {
            return null;
        }

        if (currentUser.Rolle != BereichsrolleCode.Mitarbeiter &&
            currentUser.Rolle != BereichsrolleCode.Wachleiter)
        {
            return null;
        }

        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;
        var istWachleiter = currentUser.Rolle == BereichsrolleCode.Wachleiter;

        var eintraege = new List<BereichsNavigationItemViewModel>
        {
            new()
            {
                Text = istWachleiter ? "Dienstplan" : "Dienstwünsche",
                IconCssClass = "bi bi-calendar3",
                Area = areaName,
                Controller = "Dienstplan",
                Action = "Index",
                IstAktiv =
                    string.Equals(aktuellerController, "Dienstplan", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(aktuelleAction, "Index", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(aktuelleAction, "Wachleiterkalender", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(aktuelleAction, "Monatsauswertung", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(aktuelleAction, "BuchhaltungPdf", StringComparison.OrdinalIgnoreCase))
            },
            new()
            {
                Text = "Veröffentlichter Plan",
                IconCssClass = "bi bi-clipboard2-check",
                Area = areaName,
                Controller = "Dienstplan",
                Action = "Plan",
                IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Dienstplan", "Plan")
            }
        };

        if (istWachleiter)
        {
            eintraege.Add(new BereichsNavigationItemViewModel
            {
                Text = "Autoplan",
                IconCssClass = "bi bi-magic",
                Area = areaName,
                Controller = "Autoplan",
                Action = "Index",
                IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Autoplan", "Index")
            });

            eintraege.Add(new BereichsNavigationItemViewModel
            {
                Text = "Urlaubsplaner",
                IconCssClass = "bi bi-calendar-heart",
                Area = areaName,
                Controller = "Urlaubsplaner",
                Action = "Index",
                IstAktiv =
                    string.Equals(aktuellerController, "Urlaubsplaner", StringComparison.OrdinalIgnoreCase)
            });
        }
        else if (darfMeinUrlaub)
        {
            eintraege.Add(new BereichsNavigationItemViewModel
            {
                Text = "Mein Urlaub",
                IconCssClass = "bi bi-calendar-heart",
                Area = areaName,
                Controller = "MitarbeiterUrlaub",
                Action = "Index",
                IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "MitarbeiterUrlaub", "Index")
            });
        }

        return new BereichsNavigationSectionViewModel
        {
            Titel = "Planung",
            Eintraege = eintraege
        };
    }

    private static BereichsNavigationSectionViewModel? BauePersonalSektionWennErlaubt(
        OrganisationsbereichCode navigationsBereich,
        CurrentUserContext currentUser,
        string? aktuellerController,
        string? aktuelleAction)
    {
        if (!currentUser.HatModul(ModulCode.Personal))
        {
            return null;
        }

        var scope = _benutzerScopeErmittler.ErmittleFuerBenutzerlisten(
            currentUser.Bereich,
            currentUser.Rolle);

        if (!scope.DarfBenutzerlistenLesen ||
            !scope.Zielbereich.HasValue ||
            scope.Zielbereich.Value != navigationsBereich)
        {
            return null;
        }

        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;
        var eintraege = new List<BereichsNavigationItemViewModel>();

        if (navigationsBereich != OrganisationsbereichCode.Intensivtransport)
        {
            eintraege.Add(new BereichsNavigationItemViewModel
            {
                Text = ErmittleBenutzerText(navigationsBereich),
                IconCssClass = ErmittleBenutzerIcon(navigationsBereich),
                Area = areaName,
                Controller = "Benutzer",
                Action = "Index",
                IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Benutzer", "Index")
            });
        }

        if (navigationsBereich == OrganisationsbereichCode.Intensivtransport &&
            currentUser.Rolle == BereichsrolleCode.Wachleiter)
        {
            eintraege.Add(new BereichsNavigationItemViewModel
            {
                Text = "Personaldaten",
                IconCssClass = "bi bi-person-badge",
                Area = areaName,
                Controller = "Personal",
                Action = "Index",
                IstAktiv = IstAktiv(aktuellerController, aktuelleAction, "Personal", "Index")
            });
        }

        if (!eintraege.Any())
        {
            return null;
        }

        return new BereichsNavigationSectionViewModel
        {
            Titel = navigationsBereich == OrganisationsbereichCode.Vorstand
                ? "Personalmanagement"
                : "Personal",
            Eintraege = eintraege
        };
    }

    private static BereichsNavigationSectionViewModel? BaueFahrzeugmanagementSektionWennErlaubt(
    OrganisationsbereichCode navigationsBereich,
    CurrentUserContext currentUser,
    string? aktuellerController,
    string? aktuelleAction)
    {
        if (navigationsBereich != OrganisationsbereichCode.Intensivtransport)
        {
            return null;
        }

        if (currentUser.Rolle != BereichsrolleCode.Wachleiter)
        {
            return null;
        }

        if (!currentUser.HatModul(ModulCode.Fahrzeugmanagement))
        {
            return null;
        }

        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;

        return new BereichsNavigationSectionViewModel
        {
            Titel = "Fahrzeugmanagement",
            Eintraege =
            [
                new BereichsNavigationItemViewModel
            {
                Text = "Fahrzeuge",
                IconCssClass = "bi bi-truck-front",
                Area = areaName,
                Controller = "Fahrzeuge",
                Action = "Index",
                IstAktiv =
                    string.Equals(aktuellerController, "Fahrzeuge", StringComparison.OrdinalIgnoreCase)
            },
            new BereichsNavigationItemViewModel
            {
                Text = "Tablets",
                IconCssClass = "bi bi-tablet-landscape",
                Area = areaName,
                Controller = "TrackingGeraete",
                Action = "Index",
                IstAktiv =
                    string.Equals(aktuellerController, "TrackingGeraete", StringComparison.OrdinalIgnoreCase)
            }
            ]
        };
    }


    private static BereichsNavigationSectionViewModel? BaueLagermanagementSektionWennErlaubt(
        OrganisationsbereichCode navigationsBereich,
        CurrentUserContext currentUser,
        string? aktuellerController,
        string? aktuelleAction)
    {
        if (navigationsBereich != OrganisationsbereichCode.Intensivtransport)
            return null;

        if (currentUser.Rolle != BereichsrolleCode.Wachleiter)
            return null;

        if (!currentUser.HatModul(ModulCode.Lagerlogistik))
            return null;

        var areaName = BereichsRoutingHelper.GetAreaName(navigationsBereich) ?? string.Empty;

        return new BereichsNavigationSectionViewModel
        {
            Titel = "Lager",
            Eintraege =
            [
                new BereichsNavigationItemViewModel
                {
                    Text         = "Übersicht",
                    IconCssClass = "bi bi-boxes",
                    Area         = areaName,
                    Controller   = "LagerUebersicht",
                    Action       = "Index",
                    IstAktiv     = string.Equals(aktuellerController, "LagerUebersicht", StringComparison.OrdinalIgnoreCase)
                },
                new BereichsNavigationItemViewModel
                {
                    Text         = "Artikel & Bestand",
                    IconCssClass = "bi bi-box-seam",
                    Area         = areaName,
                    Controller   = "LagerArtikel",
                    Action       = "Index",
                    IstAktiv     = string.Equals(aktuellerController, "LagerArtikel", StringComparison.OrdinalIgnoreCase)
                },
                new BereichsNavigationItemViewModel
                {
                    Text         = "O₂ Depot",
                    IconCssClass = "bi bi-capsule",
                    Area         = areaName,
                    Controller   = "Sauerstoff",
                    Action       = "Index",
                    IstAktiv     = string.Equals(aktuellerController, "Sauerstoff", StringComparison.OrdinalIgnoreCase)
                },
                new BereichsNavigationItemViewModel
                {
                    Text         = "O₂ Scannen",
                    IconCssClass = "bi bi-qr-code-scan",
                    Area         = areaName,
                    Controller   = "Sauerstoff",
                    Action       = "Scan",
                    IstAktiv     = string.Equals(aktuellerController, "Sauerstoff", StringComparison.OrdinalIgnoreCase)
                                   && string.Equals(aktuelleAction, "Scan", StringComparison.OrdinalIgnoreCase)
                },
                new BereichsNavigationItemViewModel
                {
                    Text         = "Einsatzverbrauch",
                    IconCssClass = "bi bi-journal-medical",
                    Area         = areaName,
                    Controller   = "EinsatzVerbrauch",
                    Action       = "Index",
                    IstAktiv     = string.Equals(aktuellerController, "EinsatzVerbrauch", StringComparison.OrdinalIgnoreCase)
                }
            ]
        };
    }

    private static string ErmittleDashboardIcon(OrganisationsbereichCode bereich)
    {
        return bereich switch
        {
            OrganisationsbereichCode.Intensivtransport => "bi bi-speedometer2",
            OrganisationsbereichCode.Verwaltung => "bi bi-grid-1x2-fill",
            OrganisationsbereichCode.Vorstand => "bi bi-speedometer2",
            _ => "bi bi-speedometer2"
        };
    }

    private static string ErmittleBenutzerText(OrganisationsbereichCode bereich)
    {
        return bereich switch
        {
            OrganisationsbereichCode.Intensivtransport => "Mitarbeiter",
            OrganisationsbereichCode.Verwaltung => "Personalwesen",
            OrganisationsbereichCode.Vorstand => "Personalübersicht",
            _ => "Benutzer"
        };
    }

    private static string ErmittleBenutzerIcon(OrganisationsbereichCode bereich)
    {
        return bereich switch
        {
            OrganisationsbereichCode.Intensivtransport => "bi bi-people",
            OrganisationsbereichCode.Verwaltung => "bi bi-person-vcard",
            OrganisationsbereichCode.Vorstand => "bi bi-people",
            _ => "bi bi-people"
        };
    }

    private static bool IstAktiv(
        string? aktuellerController,
        string? aktuelleAction,
        string zielController,
        string zielAction)
    {
        return string.Equals(aktuellerController, zielController, StringComparison.OrdinalIgnoreCase)
            && string.Equals(aktuelleAction, zielAction, StringComparison.OrdinalIgnoreCase);
    }
}