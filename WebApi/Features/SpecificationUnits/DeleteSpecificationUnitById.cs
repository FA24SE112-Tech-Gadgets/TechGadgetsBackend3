using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;

namespace WebApi.Features.SpecificationUnits;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
public class DeleteSpecificationUnitByIdController : ControllerBase
{
    [HttpDelete("specification-units/{id}")]
    [Tags("Specification Units")]
    [SwaggerOperation(Summary = "Delete specification unit by Id", Description = "This API is for delete specification unit by Id")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Handler([FromRoute] int id, [FromServices] AppDbContext context)
    {
        var specificationUnitExists = await context.SpecificationUnits.AnyAsync(u => u.Id == id);
        if (!specificationUnitExists)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("specificationUnit", "Không tìm thấy đơn vị")
                .Build();
        }

        var hasSpecificationValue = await context.SpecificationValues.AnyAsync(va => va.SpecificationUnitId == id);
        var hasGadgetRequestSpecifications = await context.GadgetRequestSpecifications.AnyAsync(re => re.SpecificationUnitId == id);

        if (!hasSpecificationValue && !hasGadgetRequestSpecifications)
        {
            await context.SpecificationUnits.Where(u => u.Id == id).ExecuteDeleteAsync();
        }
        else
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_02)
                .AddReason("specificationUnit", "Không thể xóa đơn vị này")
                .Build();
        }

        return NoContent();
    }
}