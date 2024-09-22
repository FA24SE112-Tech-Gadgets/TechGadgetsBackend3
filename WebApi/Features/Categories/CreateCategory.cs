using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.Categories.Mappers;
using WebApi.Features.Categories.Models;

namespace WebApi.Features.Categories;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
[RequestValidation<Request>]
public class CreateCategoryController : ControllerBase
{
    public new class Request
    {
        public int? ParentId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .WithMessage("Tên không được để trống");
        }
    }

    [HttpPost("categories")]
    [Tags("Categories")]
    [SwaggerOperation(Summary = "Create Category", Description = "This API is for creating a category")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context)
    {
        if (request.ParentId is not null
            && !await context.Categories.AnyAsync(c => c.Id == request.ParentId))
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("category", "Không tìm thấy thể loại")
                .Build();
        }

        if (await context.Categories.AnyAsync(c => c.Name == request.Name))
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_01)
                .AddReason("category", "Thể loại đã tồn tại")
                .Build();
        }

        var category = new Category
        {
            Name = request.Name,
            ParentId = request.ParentId,
            IsAdminCreated = true
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return Created("", category.ToCategoryResponse());
    }
}
