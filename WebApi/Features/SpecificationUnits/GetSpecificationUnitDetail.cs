using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Data;
using WebApi.Features.SpecificationUnits.Mappers;
using WebApi.Features.SpecificationUnits.Models;

namespace WebApi.Features.SpecificationUnits;

[ApiController]
public class GetSpecificationUnitDetailController : ControllerBase
{
    [HttpGet("specification-units/{id}")]
    [Tags("Specification Units")]
    [SwaggerOperation(Summary = "Get specification unit by Id", Description = "This API is for getting specification unit detail")]
    [ProducesResponseType(typeof(SpecificationUnitResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Handler([FromRoute] int id, [FromServices] AppDbContext context)
    {
        var specificationUnit = await context.SpecificationUnits.FirstOrDefaultAsync(su => su.Id == id);
        if (specificationUnit == null)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_00)
                .AddReason("specificationUnit", "Không tìm thấy đơn vị")
                .Build();
        }

        return Ok(specificationUnit.ToSpecificationUnitResponse());
    }
}
