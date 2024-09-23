using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Data.Entities;
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
        public SortDir? SortOrder { get; set; }
        public string? SortColumn { get; set; }
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

        var query = context.Brands.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
                            .Where(b => b.BrandCategories.Any(bc => bc.CategoryId == categoryId)
                                    && b.Name.Contains(request.Name ?? ""))
                            .Select(b => b.ToBrandResponse())
                            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<Brand, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => c => c.Name,
            _ => c => c.Id
        };
    }
}