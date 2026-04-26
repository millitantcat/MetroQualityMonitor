using MetroQualityMonitor.Application.Analytics.Models;
using MetroQualityMonitor.Application.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/clusters")]
public class ClustersController(IClusterService clusterService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StationWithClusterDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await clusterService.GetAllWithClustersAsync(cancellationToken);
        return Ok(result);
    }
}
