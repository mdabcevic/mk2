using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Interfaces
{
    public interface IProductService
    {
        Task<ServiceResult<ProductDTO?>> GetByIdAsync(int id);
        Task<ServiceResult<IEnumerable<ProductDTO>>> GetAllAsync();
        Task<ServiceResult<IEnumerable<GroupedProductsDTO>>> GetAllGroupedAsync();
        Task<ServiceResult<IEnumerable<ProductBaseDTO>>> GetFilteredAsync(string? name = null, string? category = null);
        Task<ServiceResult<IEnumerable<ProductCategoryDTO>>> GetProductCategoriesAsync();
        Task<ServiceResult> AddAsync(UpsertProductDTO product);
        Task<ServiceResult> UpdateAsync(int id, UpsertProductDTO product);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
