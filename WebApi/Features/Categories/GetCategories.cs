using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.Categories.Mappers;
using WebApi.Features.Categories.Models;

namespace WebApi.Features.Categories;

[ApiController]
[RequestValidation<Request>]
public class GetCategoriesController : ControllerBase
{
    public new class Request : PagedRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool? IsAdminCreated { get; set; }
        public bool? IsRoot { get; set; }
        public SortDir? SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    public class RequestValidator : PagedRequestValidator<Request>;

    [HttpGet("categories")]
    [Tags("Categories")]
    [SwaggerOperation(Summary = "Get Categories", Description = "This API is for retrieving categories")]
    [ProducesResponseType(typeof(PagedList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, [FromServices] AppDbContext context)
    {
        var query = context.Categories.AsQueryable();

        if (request.IsAdminCreated.HasValue)
        {
            query = query.Where(c => c.IsAdminCreated == request.IsAdminCreated.Value);
        }

        if (request.IsRoot.HasValue)
        {
            if (request.IsRoot.Value)
            {
                query = query.Where(c => c.ParentId == null);
            }
            else
            {
                query = query.Where(c => c.ParentId != null);
            }
        }

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
                            .Where(c => c.Name.Contains(request.Name))
                            .Select(c => c.ToCategoryResponse())
                            .ToPagedListAsync(request);

        return Ok(response);
    }

    private static Expression<Func<Category, object>> GetSortProperty(Request request)
    {
        return request.SortColumn?.ToLower() switch
        {
            "name" => c => c.Name,
            _ => c => c.Id
        };
    }
}
