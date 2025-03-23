using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Interfaces
{
    public interface IProductsService
    {
        Task<ProductsDTO?> GetByIdAsync(int id);
        Task<object> GetAllAsync(bool groupBy = false);
        Task<IEnumerable<ProductsBaseDTO>> GetFilteredAsync(string? name = null, string? category = null);
        Task<IEnumerable<ProductCategoryDTO>> GetProductCategoriesAsync();
        Task AddAsync(UpsertProductDTO product);
        Task UpdateAsync(int id, UpsertProductDTO product);
        Task DeleteAsync(int id);
    }
}
