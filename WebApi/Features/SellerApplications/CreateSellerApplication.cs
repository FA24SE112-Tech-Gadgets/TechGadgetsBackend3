﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Filters;
using WebApi.Data;
using WebApi.Data.Entities;
using WebApi.Services.Storage;

namespace WebApi.Features.SellerApplications;

[ApiController]
[JwtValidation]
[RolesFilter(Role.Admin)]
[RequestValidation<Request>]
public class CreateSellerApplicationController : ControllerBase
{
    public new class Request
    {
        public string? CompanyName { get; set; }
        public string ShopName { get; set; } = default!;
        public string ShippingAddress { get; set; } = default!;
        public int BusinnessModelId { get; set; }
        public IFormFile? BusinessRegistrationCertificate { get; set; }
        public string TaxCode { get; set; } = default!;
        public string? RejectReason { get; set; }
        public SellerApplicationType Type { get; set; }
        public ICollection<string> BillingMails { get; set; } = [];
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(sp => sp.CompanyName)
                .NotEmpty()
                .When(sp => RequiresCompanyName(sp.BusinnessModelId))
                .WithMessage("Tên công ty không được để trống");
            RuleFor(sp => sp.BusinessRegistrationCertificate)
                .NotNull()
                .When(sp => RequiresCertificate(sp.BusinnessModelId))
                .WithMessage("Giấy phép kinh doanh không được để trống");
            RuleFor(sp => sp.TaxCode)
                .NotEmpty()
                .WithMessage("Mã số thuế không được để trống")
                .Must(BeValidTaxCode)
                .WithMessage("Mã số thuế không hợp lệ.");
            RuleFor(sp => sp.ShippingAddress)
                .NotEmpty()
                .WithMessage("Địa chỉ lấy hàng không được để trống");
            RuleFor(sp => sp.ShopName)
                .NotEmpty()
                .WithMessage("Tên cửa hàng không được để trống");
            RuleFor(sp => sp.Type)
                .NotNull()
                .When(sp => sp.Type == SellerApplicationType.Create)
                .WithMessage("Loại đơn phải là Create");
        }
        private static bool RequiresCompanyName(int businessModelId)
        {
            var modelsRequiringCompanyName = new List<int> { 2, 3 }; //Hộ kinh doanh, Công ty
            return modelsRequiringCompanyName.Contains(businessModelId);
        }

        private static bool RequiresCertificate(int businessModelId)
        {
            var modelsRequiringCompanyName = new List<int> { 2, 3 }; //Hộ kinh doanh, Công ty
            return modelsRequiringCompanyName.Contains(businessModelId);
        }

        private static bool BeValidTaxCode(string taxCode)
        {
            //Danh sách mã 63 tỉnh thành VN
            var ValidProvinceCodes = new List<string> {
                "01", "02", "04", "06", "08", "10", "11", "12", "14",
                "15", "17", "19", "20", "22", "24", "25", "26", "27",
                "30", "31", "33", "34", "35", "36", "37", "38", "40",
                "42", "44", "45", "46", "48", "49", "51", "52", "54",
                "56", "58", "60", "62", "64", "66", "67", "68", "70",
                "72", "74", "75", "77", "79", "80", "82", "83", "84",
                "86", "87", "89", "91", "92", "93", "94", "95", "96"
            };
            // Kiểm tra độ dài của mã số thuế
            if (taxCode.Length != 10)
            {
                return false;
            }

            // Kiểm tra 2 chữ số đầu tiên (N1N2) có nằm trong danh sách mã tỉnh hợp lệ không
            string provinceCode = taxCode.Substring(0, 2);
            if (!ValidProvinceCodes.Contains(provinceCode))
            {
                return false;
            }

            // Lấy các số từ N1 đến N9 để tính toán
            string coreTaxCode = taxCode.Substring(0, 9);
            int checkDigit = int.Parse(taxCode[9].ToString());

            // Kiểm tra chữ số kiểm tra (N10) theo thuật toán
            return IsValidCheckDigit(coreTaxCode, checkDigit);
        }

        private static bool IsValidCheckDigit(string coreTaxCode, int checkDigit)
        {
            // Thuật toán kiểm tra mã số thuế (có thể thay đổi theo quy định cụ thể)
            // Ví dụ thuật toán kiểm tra tính hợp lệ N10
            int[] weights = [31, 29, 23, 19, 17, 13, 7, 5, 3];
            int sum = 0;

            for (int i = 0; i < coreTaxCode.Length; i++)
            {
                sum += int.Parse(coreTaxCode[i].ToString()) * weights[i];
            }

            int calculatedCheckDigit = sum % 11;
            return calculatedCheckDigit == checkDigit;
        }
    }

    [HttpPost("seller-applications")]
    [Tags("Seller Applications")]
    [SwaggerOperation(
        Summary = "Create Seller Application",
        Description = "API is for register seller. Note:" +
                            "<br>&nbsp; - Cá nhân(Personal) thì không cần truyền CompanyName và BusinessRegistrationCertificate." +
                            "<br>&nbsp; - Mã số thuế được duplicate(cho đơn giản) và format được quy định trong Business Rules." +
                            "<br>&nbsp; - Địa chỉ lấy hàng (ShippingAddress) khác với địa chỉ trong User (2 địa chỉ này có thể trùng được nhưng nghĩa khác nhau)."
    )]
    public async Task<IActionResult> Handler([FromForm] Request request, AppDbContext context, GoogleStorageService storageService)
    {

        return Created();
    }
}
