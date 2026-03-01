using Microsoft.AspNetCore.Mvc;
using PBMDashboard.Api.Services;

namespace PBMDashboard.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly PbmDashboardService _dashboardService;

    public DashboardController(PbmDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("pbm")]
    public async Task<IActionResult> GetPbmDashboard(CancellationToken cancellationToken)
    {
        var response = await _dashboardService.BuildAsync(cancellationToken);
        return Ok(response);
    }
}
