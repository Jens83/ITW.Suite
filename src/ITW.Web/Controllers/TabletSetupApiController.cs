using ITW.Fahrzeugmanagement.Application.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers;

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/tablet/setup")]
public sealed class TabletSetupApiController : ControllerBase
{
    private readonly CompleteTrackingGeraetSetupService _completeSetupService;

    public TabletSetupApiController(CompleteTrackingGeraetSetupService completeSetupService)
    {
        _completeSetupService = completeSetupService
            ?? throw new ArgumentNullException(nameof(completeSetupService));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromBody] TabletSetupCompleteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new
            {
                success = false,
                message = "Der Request-Body ist erforderlich."
            });
        }

        var result = await _completeSetupService.ExecuteAsync(
            new CompleteTrackingGeraetSetupCommand(request.Einrichtungscode),
            cancellationToken);

        if (!result.IsSuccess ||
            string.IsNullOrWhiteSpace(result.DeviceIdentifier) ||
            string.IsNullOrWhiteSpace(result.ApiKey))
        {
            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage ?? "Das Tablet konnte nicht eingerichtet werden."
            });
        }

        return Ok(new
        {
            success = true,
            deviceIdentifier = result.DeviceIdentifier,
            apiKey = result.ApiKey
        });
    }
}

public sealed class TabletSetupCompleteRequest
{
    public string Einrichtungscode { get; set; } = string.Empty;
}