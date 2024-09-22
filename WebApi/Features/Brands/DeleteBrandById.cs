using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Services.Storage;

namespace WebApi.Features.Brands;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
public class DeleteBrandByIdController : ControllerBase
{
    [HttpDelete("brands/{id}")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Delete Brand", Description = "This API is to delete brand by id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Handler(int id, AppDbContext context, GoogleStorageService storageService)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        if (brand is null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("brand", "Không tìm thấy thương hiệu")
                .Build();
        }

        var anyGadget = await context.Gadgets.AnyAsync(g => g.BrandId == id);
        var anyGadgetRequestBrand = await context.GadgetRequestBrands.AnyAsync(g => g.BrandId == id);

        if (anyGadget || anyGadgetRequestBrand)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_02)
                .AddReason("brand", "Không thể xóa thương hiệu này do nó đang được tham chiếu")
                .Build();
        }

        await context.Brands.Where(b => b.Id == id).ExecuteDeleteAsync();
        await storageService.DeleteFileFromCloudStorage(brand.LogoUrl);

        return NoContent();
    }
}
