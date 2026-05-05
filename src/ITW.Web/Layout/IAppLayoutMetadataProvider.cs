namespace ITW.Web.Layout;

public interface IAppLayoutMetadataProvider
{
    AppLayoutMetadata GetForArea(string areaName);
}
