using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.Services
{
    public class ProductService(
        IRepository<Products> repository, 
        IRepository<ProductCategory> categoryRepository, 
        IMapper mapper) : IProductService
    {
        public async Task<ServiceResult<ProductDTO?>> GetByIdAsync(int id)
        {
            try
            {
                var product = await repository.GetByIdAsync(id);
                if (product == null)
                    return ServiceResult<ProductDTO?>.Fail($"Product with id {id} not found", ErrorType.NotFound);

                var dto = mapper.Map<ProductDTO>(product);
                return ServiceResult<ProductDTO?>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDTO?>.Fail($"An error occurred while fetching the product: {ex.Message}", ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductDTO>>> GetAllAsync()
        {
            try
            {
                var products = await repository.GetAllAsync(true, p => p.Name);

                if (!products.Any())
                    return ServiceResult<IEnumerable<ProductDTO>>.Fail("There are currently no products", ErrorType.NotFound);

                var dto = mapper.Map<IEnumerable<ProductDTO>>(products);
                return ServiceResult<IEnumerable<ProductDTO>>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProductDTO>>.Fail($"An error occurred while fetching the products: {ex.Message}", ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult<IEnumerable<GroupedProductsDTO>>> GetAllGroupedAsync()
        {
            try
            {
                var groupedProducts = await categoryRepository.GetAllAsync(true);

                if (!groupedProducts.Any())
                    return ServiceResult<IEnumerable<GroupedProductsDTO>>.Fail("There are currently no products", ErrorType.NotFound);

                var sortedProducts = groupedProducts
                    .Select(gp => new GroupedProductsDTO
                    {
                        Category = gp.Name,
                        Products = gp.Products
                        .OrderBy(p => p.Name)
                        .Select(p => mapper.Map<ProductBaseDTO>(p))
                    }).ToList();

                return ServiceResult<IEnumerable<GroupedProductsDTO>>.Ok(sortedProducts);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<GroupedProductsDTO>>.Fail($"An error occurred while fetching the products: {ex.Message}", ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductBaseDTO>>> GetFilteredAsync(string? name = null, string? category = null)
        {
            try
            {
                var query = repository.Query();
                if (name != null)
                    query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));

                if (category != null)
                    query = query.Where(x => x.Category.Name.ToLower().Contains(category.ToLower()));

                var products = await query.ToListAsync();

                if (!products.Any())
                    return ServiceResult<IEnumerable<ProductBaseDTO>>.Fail("No products found matching the criteria", ErrorType.NotFound);

                var dto = mapper.Map<IEnumerable<ProductBaseDTO>>(products);
                return ServiceResult<IEnumerable<ProductBaseDTO>>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProductBaseDTO>>.Fail($"An error occurred while fetching the products: {ex.Message}", ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult> AddAsync(UpsertProductDTO product)
        {
            try
            {
                await ValidateProductAsync(product);
            
                var existingProduct = await repository.ExistsAsync(p =>
                    p.Name.ToLower() == product.Name.ToLower() &&
                    (p.Volume == null && product.Volume == null ||
                     p.Volume != null && product.Volume != null && p.Volume.ToLower() == product.Volume.ToLower())
                );

                if (existingProduct)
                    return ServiceResult.Fail($"Product with name '{product.Name}' and volume '{product.Volume}' already exists.", ErrorType.Conflict);

                var newProduct = mapper.Map<Products>(product);
                await repository.AddAsync(newProduct);
                return ServiceResult.Ok();
            }
            catch (Exception ex) when (ex is NotFoundException or ValidationException)
            {
                var errorType = ex is NotFoundException ? ErrorType.NotFound : ErrorType.Validation;
                return ServiceResult.Fail(ex.Message, errorType);
            }
            catch (Exception ex) {
                return ServiceResult.Fail(ex.Message, ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpsertProductDTO product)
        {
            try
            {
                await ValidateProductAsync(product);

                var updateProduct = await GetProductByIdAsync(id);

                var existingProduct = await repository.ExistsAsync(p =>
                    p.Id != id &&
                    p.Name.ToLower() == product.Name.ToLower() &&
                    (p.Volume == null && product.Volume == null ||
                     p.Volume != null && product.Volume != null && p.Volume.ToLower() == product.Volume.ToLower())
                );

                if (existingProduct)
                    return ServiceResult.Fail($"Product with name '{product.Name}' and volume '{product.Volume}' already exists.", ErrorType.Conflict);

                mapper.Map(product, updateProduct);
                await repository.UpdateAsync(updateProduct);
                return ServiceResult.Ok();
            }
            catch (Exception ex) when (ex is NotFoundException or ValidationException)
            {
                var errorType = ex is NotFoundException ? ErrorType.NotFound : ErrorType.Validation;
                return ServiceResult.Fail(ex.Message, errorType);
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(ex.Message, ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var product = await GetProductByIdAsync(id);
          
                await repository.DeleteAsync(product);
                return ServiceResult.Ok();
            }
            catch (NotFoundException ex)
            {
                return ServiceResult.Fail(ex.Message, ErrorType.NotFound);
            }
            catch (Exception ex) {
                return ServiceResult.Fail(ex.Message, ErrorType.Unknown);
            }
        }

        public async Task<ServiceResult<IEnumerable<ProductCategoryDTO>>> GetProductCategoriesAsync()
        {
            try
            {
                var categories = await categoryRepository.GetAllAsync();
                var dto = mapper.Map<IEnumerable<ProductCategoryDTO>>(categories);
                return ServiceResult<IEnumerable<ProductCategoryDTO>>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<ProductCategoryDTO>>.Fail($"An error occurred while fetching the categories: {ex.Message}", ErrorType.Unknown);
            }
        }

        private async Task ValidateProductAsync(UpsertProductDTO product)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ValidationException("Product name is required.");
            }

            bool categoryExists = await categoryRepository.ExistsAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                throw new ValidationException($"Product category id {product.CategoryId} doesn't exist");
            }
        }

        private async Task<Products> GetProductByIdAsync(int id)
        {
            var product = await repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with id {id} not found");
            }
            return product;
        }
    }
}
