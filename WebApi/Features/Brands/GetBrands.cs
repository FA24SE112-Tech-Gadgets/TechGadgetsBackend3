using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Features.Brands.Mappers;
using WebApi.Features.Brands.Models;

namespace WebApi.Features.Brands;

[ApiController]
[RequestValidation<Request>]
public class GetBrandsController : ControllerBase
{
    public new class Request : PagedRequest
    {
        public string? Name { get; set; }
    }

    public class Validator : PagedRequestValidator<Request>;

    [HttpGet("brands")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Get Brands", Description = "This API is to get brands")]
    [ProducesResponseType(typeof(PagedList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, AppDbContext context)
    {
        var response = await context.Brands
                                .Where(b => b.Name.Contains(request.Name ?? ""))
                                .OrderBy(b => b.Name)
                                .Select(b => b.ToBrandResponse())
                                .ToPagedListAsync(request);

        return Ok(response);
    }
}