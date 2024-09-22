using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.BrandCategories.Mappers;
using WebApi.Features.BrandCategories.Models;

namespace WebApi.Features.BrandCategories;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
[RequestValidation<Request>]
public class CreateBrandCategoryController : ControllerBase
{
    public new class Request
    {
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.BrandId)
                .NotNull()
                .WithMessage("Id thương hiệu không được để trống");

            RuleFor(r => r.CategoryId)
                .NotNull()
                .WithMessage("Id thể loại không được để trống");
        }
    }

    [HttpPost("brand-categories")]
    [Tags("Brand Categories")]
    [SwaggerOperation(Summary = "Create BrandCategory", Description = "This API is to create a brandCategory")]
    [ProducesResponseType(typeof(BrandCategoryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Handler(Request request, AppDbContext context)
    {
        var brand = await context.Brands.FindAsync(request.BrandId);
        if (brand == null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("brand", "Không tìm thấy thương hiệu")
                .Build();
        }

        var category = await context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("category", "Không tìm thấy thể loại")
                .Build();
        }

        if (await context.BrandCategories.AnyAsync(bc => bc.BrandId == request.BrandId
                                                        && bc.CategoryId == request.CategoryId))
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_01)
                .AddReason("brandCategory", "Đã tồn tại mối liên hệ")
                .Build();
        }

        var brandCategory = new BrandCategory
        {
            CategoryId = request.CategoryId!.Value,
            BrandId = request.BrandId!.Value,
            Brand = brand,
            Category = category
        };

        context.BrandCategories.Add(brandCategory);
        await context.SaveChangesAsync();

        return Created("", brandCategory.ToBrandCategoryResponse());
    }
}
