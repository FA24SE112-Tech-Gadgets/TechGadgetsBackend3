﻿using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Expressions;
using WebApi.Common.Filters;
using WebApi.Common.Paginations;
using WebApi.Common.QueryableExtensions;
using WebApi.Data;
using WebApi.Data.Entities;
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
        public string? SortOrder { get; set; }
        public string? SortColumn { get; set; }
    }

    public class Validator : PagedRequestValidator<Request>;

    [HttpGet("brands")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Get Brands", Description = "This API is to get brands")]
    [ProducesResponseType(typeof(PagedList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromQuery] Request request, AppDbContext context)
    {
        var query = context.Brands.AsQueryable();

        query = query.OrderByColumn(GetSortProperty(request), request.SortOrder);

        var response = await query
                            .Where(b => b.Name.Contains(request.Name ?? ""))
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