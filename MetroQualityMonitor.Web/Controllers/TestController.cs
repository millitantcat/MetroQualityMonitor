using MetroQualityMonitor.Application.Test;
using MetroQualityMonitor.Application.Test.Models;
using MetroQualityMonitor.Application.Test.Services;
using Microsoft.AspNetCore.Mvc;

namespace MetroQualityMonitor.Web.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(ITestAppService testAppService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TestEntityDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await testAppService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TestEntityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await testAppService.GetByIdAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TestEntityDto>> Create(
        [FromBody] CreateTestEntityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await testAppService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}