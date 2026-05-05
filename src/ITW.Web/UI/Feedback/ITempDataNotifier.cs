namespace ITW.Web.UI.Feedback;

public interface ITempDataNotifier
{
    void Success(string message);
    void Error(string message);
    void Info(string message);
}
