using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Features.SpecificationUnits.Mappers;
using WebApi.Features.SpecificationUnits.Models;

namespace WebApi.Features.SpecificationUnits;

[ApiController]
public class GetSpecificationUnitsController : ControllerBase
{
    public new class Request : PagedRequest
    {
        public string? Name { get; set; }
    }

    [HttpGet("specification-units")]
    [Tags("Specification Units")]
    [SwaggerOperation(Summary = "List of Specification Units", Description = "Get list of specification units or get by unit's name")]
    [ProducesResponseType(typeof(PagedList<SpecificationUnitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context)
    {
        var response = await context.SpecificationUnits
            .Where(u => u.Name.Contains(request.Name ?? string.Empty))
            .Select(u => u.ToSpecificationUnitResponse())
            .ToPagedListAsync(request);

        return Ok(response);
    }
}