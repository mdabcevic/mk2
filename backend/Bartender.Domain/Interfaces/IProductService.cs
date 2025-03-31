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
        Task<ServiceResult<ProductDto?>> GetByIdAsync(int id);
        Task<ServiceResult<List<ProductDto>>> GetAllAsync(bool? exclusive = null);
        Task<ServiceResult<List<GroupedProductsDto>>> GetAllGroupedAsync(bool? exclusive = null);
        Task<ServiceResult<List<ProductBaseDto>>> GetFilteredAsync(string? name = null, string? category = null);
        Task<ServiceResult<List<ProductCategoryDto>>> GetProductCategoriesAsync();
        Task<ServiceResult> AddAsync(UpsertProductDto product);
        Task<ServiceResult> UpdateAsync(int id, UpsertProductDto product);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
