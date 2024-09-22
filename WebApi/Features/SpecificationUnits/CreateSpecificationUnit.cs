using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.SpecificationUnits.Mappers;
using WebApi.Features.SpecificationUnits.Models;

namespace WebApi.Features.SpecificationUnits;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
public class CreateSpecificationUnitController : ControllerBase
{
    public new record Request(string Name);

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .WithMessage("Tên không được để trống");
        }
    }

    [HttpPost("specification-units")]
    [Tags("Specification Units")]
    [SwaggerOperation(Summary = "Create specification unit", Description = "This API is for Admin to create a specification unit")]
    [ProducesResponseType(typeof(SpecificationUnitResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context)
    {
        var isDuplicated = await context.SpecificationUnits.AnyAsync(u => u.Name == request.Name);
        if (isDuplicated)
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_01)
                .AddReason("name", "Tên này đã được tạo trước đó.")
                .Build();
        }

        var specificationUnit = new SpecificationUnit
        {
            Name = request.Name,
        };

        context.SpecificationUnits.Add(specificationUnit);
        await context.SaveChangesAsync();

        return Created("", specificationUnit.ToSpecificationUnitResponse());
    }
}