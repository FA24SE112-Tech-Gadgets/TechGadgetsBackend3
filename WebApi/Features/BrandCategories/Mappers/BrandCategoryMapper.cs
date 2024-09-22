using WebApi.Data.Entities;
using WebApi.Features.BrandCategories.Models;
using WebApi.Features.Brands.Mappers;
using WebApi.Features.Categories.Mappers;

namespace WebApi.Features.BrandCategories.Mappers;

public static class BrandCategoryMapper
{
    public static BrandCategoryResponse? ToBrandCategoryResponse(this BrandCategory? brandCategory)
    {
        if (brandCategory != null)
        {
            return new BrandCategoryResponse
            {
                Brand = brandCategory.Brand.ToBrandResponse()!,
                Category = brandCategory.Category.ToCategoryResponse()!,
                BrandId = brandCategory.BrandId,
                CategoryId = brandCategory.CategoryId,
            };
        }
        return null;
    }
}
