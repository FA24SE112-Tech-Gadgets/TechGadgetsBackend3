﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Exceptions;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Features.Brands.Mappers;
using WebApi.Features.Brands.Models;
using WebApi.Services.Storage;

namespace WebApi.Features.Brands;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
[RequestValidation<Request>]
public class CreateBrandController : ControllerBase
{
    public new class Request
    {
        public string Name { get; set; } = default!;
        public IFormFile Logo { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .WithMessage("Tên không được để trống");

            RuleFor(r => r.Logo)
                .NotNull()
                .WithMessage("Logo không được để trống");
        }
    }

    [HttpPost("brands")]
    [Tags("Brands")]
    [SwaggerOperation(Summary = "Create Brand", Description = "This API is to create a brand")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Handler([FromForm] Request request, AppDbContext context, GoogleStorageService storageService)
    {
        if (await context.Brands.AnyAsync(b => b.Name == request.Name))
        {
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WEB_01)
                .AddReason("name", "Tên thương hiệu đã tồn tại")
                .Build();
        }

        string? logoUrl = null;

        try
        {
            logoUrl = await storageService.UploadFileToCloudStorage(request.Logo, Guid.NewGuid().ToString());
        }
        catch (Exception)
        {
            if (logoUrl != null)
            {
                await storageService.DeleteFileFromCloudStorage(logoUrl);
            }
            throw TechGadgetException.NewBuilder()
                .WithCode(TechGadgetErrorCode.WES_00)
                .AddReason("logo", "Lỗi khi lưu logo")
                .Build();
        }

        var brand = new Brand
        {
            Name = request.Name,
            LogoUrl = logoUrl
        };

        context.Brands.Add(brand);
        await context.SaveChangesAsync();

        return Created("", brand.ToBrandResponse());
    }
}