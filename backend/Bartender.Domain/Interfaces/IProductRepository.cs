using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetAllProductsAsync(int? businessId = null, bool? exclusive = null, bool? isAdmin = null, int? page = null);
    Task<Dictionary<ProductCategory, List<Product>>> GetProductsGroupedAsync(int? businessId = null, bool? exclusive = null, bool? isAdmin = null, int? page = null);
    Task<List<Product>> GetProductsFilteredAsync(int? businessId = null, bool? exclusive = null, bool? isAdmin = null, string? name = null, string? category = null, int ? page = null);
    Task<bool> ProductExists(string name, int? productId = null, int? businessId = null, string? volume = null);
}
