using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ITW.Web.UI.Feedback;

public sealed class TempDataNotifier : ITempDataNotifier
{
    private readonly ITempDataDictionary _tempData;

    public TempDataNotifier(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory factory)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        ArgumentNullException.ThrowIfNull(factory);
        _tempData = factory.GetTempData(httpContextAccessor.HttpContext!);
    }

    public void Success(string message) => _tempData[FlashKeys.Success] = message;
    public void Error(string message) => _tempData[FlashKeys.Error] = message;
    public void Info(string message) => _tempData[FlashKeys.Info] = message;
}
