using ITW.Fahrzeugmanagement.Application.Tracking;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Area("Intensivtransport")]
[Route("api/intensivtransport/fahrzeugmanagement/location-update")]
public sealed class LocationUpdateController : ControllerBase
{
    private const string DeviceIdHeaderName = "X-Device-Id";
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly SaveLocationUpdateService _saveLocationUpdateService;

    public LocationUpdateController(SaveLocationUpdateService saveLocationUpdateService)
    {
        ArgumentNullException.ThrowIfNull(saveLocationUpdateService);
        _saveLocationUpdateService = saveLocationUpdateService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] LocationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Der Request-Body ist erforderlich." });
        }

        var deviceIdentifier = Request.Headers[DeviceIdHeaderName].ToString();
        var apiKey = Request.Headers[ApiKeyHeaderName].ToString();

        var result = await _saveLocationUpdateService.ExecuteAsync(
            new SaveLocationUpdateCommand(
                deviceIdentifier,
                apiKey,
                request.Latitude,
                request.Longitude,
                request.SpeedKmh,
                request.ErfasstAmUtc),
            cancellationToken);

        if (result.IsUnauthorized)
        {
            return Unauthorized(new
            {
                message = result.ErrorMessage ?? "Das Gerät ist nicht autorisiert."
            });
        }

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                message = result.ErrorMessage ?? "Das Standortupdate konnte nicht verarbeitet werden."
            });
        }

        return Ok(new
        {
            success = true,
            trackingGeraetId = result.TrackingGeraetId,
            deviceIdentifier = result.DeviceIdentifier,
            routeSessionId = result.RouteSessionId,
            historieGeschrieben = result.HistorieGeschrieben
        });
    }
}