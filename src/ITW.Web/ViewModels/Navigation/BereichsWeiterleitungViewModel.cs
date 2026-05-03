namespace ITW.Web.ViewModels.Navigation;

public sealed class BereichsWeiterleitungViewModel
{
    public string Titel { get; set; } = "Weiterleitung";

    public string FunktionsName { get; set; } = string.Empty;

    public string AufgerufenerBereichName { get; set; } = string.Empty;

    public string ZielbereichName { get; set; } = string.Empty;

    public string Hinweistext { get; set; } = string.Empty;

    public string ZielUrl { get; set; } = string.Empty;

    public string ButtonText { get; set; } = "Sofort wechseln";

    public int VerzogerungInMillisekunden { get; set; } = 5000;
}