namespace ITW.Web.Configuration.Bootstrap;

public sealed class InitialIdentityBootstrapOptions
{
    public bool Enabled { get; set; }

    public List<InitialIdentityUserOptions> Users { get; set; } = new();
}