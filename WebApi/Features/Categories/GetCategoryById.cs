using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.Categories.Mappers;
using WebApi.Features.Categories.Models;

namespace WebApi.Features.Categories;

[ApiController]
public class GetCategoryByIdController : ControllerBase
{
    [HttpGet("categories/{id}")]
    [Tags("Categories")]
    [SwaggerOperation(Summary = "Get Category By ID", Description = "This API is to get category details by ID")]
    [ProducesResponseType(typeof(CategoryDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(int id, [FromServices] AppDbContext context)
    {
        var category = await context.Categories
                            .Include(c => c.Parent)
                            .Include(c => c.Children)
                            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("category", "Không tìm thấy thể loại")
                .Build();
        }

        var parents = new List<Category>();

        if (category.ParentId.HasValue)
        {
            parents = await GetAllParentsRawSqlAsync(category.ParentId.Value, context);
        }

        var response = new CategoryDetailResponse
        {
            Name = category.Name,
            Id = id,
            IsAdminCreated = true,
            Parent = parents.Select(p => p.ToCategoryResponse()).ToList(),
            Children = category.Children.Select(p => p.ToCategoryResponse()).ToList(),
        };

        return Ok(response);
    }

    private static async Task<List<Category>> GetAllParentsRawSqlAsync(int categoryId, AppDbContext context)
    {
        var sql = @"
            WITH RECURSIVE CategoryHierarchy AS (
                SELECT * FROM ""Category"" WHERE ""Id"" = @categoryId
                UNION ALL
                SELECT c.* FROM ""Category"" c
                INNER JOIN CategoryHierarchy ch ON c.""Id"" = ch.""ParentId""
            )
            SELECT * FROM CategoryHierarchy;";

        var categories = await context.Categories
            .FromSqlRaw(sql, new NpgsqlParameter("categoryId", categoryId))
            .AsNoTracking()
            .ToListAsync();

        categories.Reverse();
        return categories;
    }

    private static async Task<List<Category>> LoadParentsAsync(int categoryId, AppDbContext context)
    {
        var category = await context.Categories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null) return [];

        var parents = new List<Category>();

        if (category.ParentId.HasValue)
        {
            var parentCategories = await LoadParentsAsync(category.ParentId.Value, context);
            parents.AddRange(parentCategories);
        }

        parents.Add(category);
        return parents;
    }
}
