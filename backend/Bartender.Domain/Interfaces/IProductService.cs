using Bartender.Domain.DTO.Product;

namespace Bartender.Domain.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto?> GetByIdAsync(int id);
        Task<List<ProductDto>> GetAllAsync(bool? exclusive = null);
        Task<List<GroupedProductsDto>> GetAllGroupedAsync(bool? exclusive = null);
        Task<List<ProductBaseDto>> GetFilteredAsync(bool? exclusive = null, string? name = null, string? category = null);
        Task<List<ProductCategoryDto>> GetProductCategoriesAsync();
        Task AddAsync(UpsertProductDto product);
        Task UpdateAsync(int id, UpsertProductDto product);
        Task DeleteAsync(int id);
    }
}
