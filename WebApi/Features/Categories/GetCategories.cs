using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Features.Categories.Mappers;
using WebApi.Features.Categories.Models;

namespace WebApi.Features.Categories;

[ApiController]
[RequestValidation<Request>]
public class GetCategoriesController : ControllerBase
{
    public new class Request : PagedRequest
    {
        public string? Name { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("categories")]
    [Tags("Categories")]
    [SwaggerOperation(Summary = "Get Categories", Description = "This API is for retrieving categories")]
    [ProducesResponseType(typeof(PagedList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([AsParameters] Request request, [FromServices] AppDbContext context)
    {
        var response = await context.Categories
                                .Where(c => c.Name.Contains(request.Name ?? ""))
                                .Select(c => c.ToCategoryResponse())
                                .ToPagedListAsync(request);

        return Ok(response);
    }
}
