using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Features.Brands.Mappers;
using WebApi.Features.Brands.Models;

namespace WebApi.Features.Brands;

[ApiController]
[RequestValidation<Request>]
public class GetBrandsByCategoryIdController : ControllerBase
{
    public new class Request : PagedRequest
    {
        public string? Name { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("brands/categories/{categoryId}")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Get Brands by Category Id", Description = "This API is to get brands by category id")]
    [ProducesResponseType(typeof(PagedList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(int categoryId, [FromQuery] Request request, AppDbContext context)
    {
        if (!await context.Categories.AnyAsync(c => c.Id == categoryId))
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("category", "Không tìm thấy thể loại")
                .Build();
        }

        var response = await context.Brands
                                .Where(b => b.BrandCategories.Any(bc => bc.CategoryId == categoryId)
                                        && b.Name.Contains(request.Name ?? ""))
                                .OrderBy(b => b.Name)
                                .Select(b => b.ToBrandResponse())
                                .ToPagedListAsync(request);

        return Ok(response);
    }
}