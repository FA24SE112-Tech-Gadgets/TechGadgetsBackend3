using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Data;
using WebApi.Features.Brands.Mappers;
using WebApi.Features.Brands.Models;

namespace WebApi.Features.Brands;

[ApiController]
public class GetBrandByIdController : ControllerBase
{
    [HttpGet("brands/{id}")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Get Brand", Description = "This API is to get brand by id")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler(int id, AppDbContext context)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(b => b.Id == id);

        if (brand is null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("brand", "Không tìm thấy thương hiệu")
                .Build();
        }

        return Ok(brand.ToBrandResponse());
    }
}
