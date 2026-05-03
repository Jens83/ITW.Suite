// Datei: src/ITW.Web/ViewModels/Security/PasswortResetAnfrageEintragViewModel.cs
namespace ITW.Web.ViewModels.Security;

public sealed class PasswortResetAnfrageEintragViewModel
{
    public Guid AnfrageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Vollname { get; set; } = string.Empty;

    public DateTimeOffset AngefordertAm { get; set; }
}