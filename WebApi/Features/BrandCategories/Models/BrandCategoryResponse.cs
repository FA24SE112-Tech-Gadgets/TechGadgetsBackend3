using WebApi.Features.Brands.Models;
using WebApi.Features.Categories.Models;

namespace WebApi.Features.BrandCategories.Models;

public class BrandCategoryResponse
{
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public BrandResponse Brand { get; set; } = default!;
    public CategoryResponse Category { get; set; } = default!;
}
