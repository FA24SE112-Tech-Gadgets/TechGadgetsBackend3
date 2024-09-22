using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;

namespace WebApi.Features.Categories;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
public class DeleteCategoryByIdController : ControllerBase
{
    [HttpDelete("categories/{id}")]
    [Tags("Categories")]
    [SwaggerOperation(Summary = "Delete Category", Description = "This API is to delete category by id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Handler(int id, AppDbContext context)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("category", "Không tìm thấy thể loại")
                .Build();
        }

        var hasChildren = await context.Categories.AnyAsync(c => c.ParentId == id);
        var hasGadgets = await context.Gadgets.AnyAsync(g => g.CategoryId == id);
        var hasGadgetRequestCategories = await context.GadgetRequestCategories.AnyAsync(grc => grc.CategoryId == id);
        var hasSpecificationDefinitions = await context.SpecificationDefinitions.AnyAsync(sd => sd.CategoryId == id);

        if (hasChildren || hasGadgets || hasGadgetRequestCategories || hasSpecificationDefinitions)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_02)
                .AddReason("category", "Không thể xóa thể loại này do nó đang được tham chiếu")
                .Build();
        }

        await context.Categories.Where(c => c.Id == id).ExecuteDeleteAsync();

        return NoContent();
    }
}
