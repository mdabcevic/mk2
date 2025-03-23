using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Bartender.Domain.Helpers;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.Services
{
    public class ProductsService(
        IRepository<Products> repository, 
        IRepository<ProductCategory> categoryRepository, 
        IMapper mapper) : IProductsService
    {
        public async Task<ProductsDTO?> GetByIdAsync(int id)
        {
            var product = await repository.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with id {id} not found");
            }
            return mapper.Map<ProductsDTO>(product);
        }
        public async Task<object> GetAllAsync(bool groupBy = false)
        {
            if (groupBy)
            {
                var groupedProducts = await categoryRepository.GetAllWithDetailsAsync();
                return mapper.Map<IEnumerable<GroupedProductsDTO>>(groupedProducts);
            }

            var products = await repository.GetAllWithDetailsAsync();

            if (!products.Any())
            {
                throw new NotFoundException("There are currently no products");
            }
            return mapper.Map<IEnumerable<ProductsDTO>>(products);
        }

        public async Task<IEnumerable<ProductsBaseDTO>> GetFilteredAsync(string? name = null, string? category = null)
        {
            var query = repository.Query();
            if (name != null)
                query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));

            if (category != null)
                query = query.Where(x => x.Category.Name.ToLower().Contains(category.ToLower()));

            var products = await query.ToListAsync();

            if (!products.Any())
            {
                throw new NotFoundException("No products found matching the criteria");
            }
            return mapper.Map<IEnumerable<ProductsBaseDTO>>(products);
        }

        public async Task<IEnumerable<ProductsDTO>> GetSortedByNameAsync()
        {
            var query = repository.Query()
                .Include(p => p.Category)
                .OrderBy(x => x.Name);

            var products = await query.ToListAsync();
            if (!products.Any())
            {
                throw new NotFoundException("There are currently no products");
            }
            
            return mapper.Map<IEnumerable<ProductsDTO>>(products);
        }

        public async Task AddAsync(UpsertProductDTO product)
        {
            await ValidateProduct(product);

            var existingProduct = await repository.Query()
                .Where(p => p.Name.ToLower() == product.Name.ToLower())
                .FirstOrDefaultAsync();
            if (existingProduct != null)
            {
                throw new DuplicateEntryException($"Product with name '{product.Name}' already exists.");
            }
            var newProduct = mapper.Map<Products>(product);
            await repository.AddAsync(newProduct);
        }

        public async Task UpdateAsync(int id, UpsertProductDTO product)
        {
            await ValidateProduct(product);

            var updateProduct = await GetProductByIdAsync(id);

            var existingProduct = await repository.Query()
                .Where(p => p.Name.ToLower() == product.Name.ToLower())
                .FirstOrDefaultAsync(); 
            if (existingProduct != null)
            {
                throw new DuplicateEntryException($"Product with name '{product.Name}' already exists.");
            }

            mapper.Map(product, updateProduct);
            await repository.UpdateAsync(updateProduct);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await GetProductByIdAsync(id);
            await repository.DeleteAsync(product);
        }

        public async Task<IEnumerable<ProductCategoryDTO>> GetProductCategoriesAsync()
        {
            var categories = await categoryRepository.GetAllAsync();
            return mapper.Map<IEnumerable<ProductCategoryDTO>>(categories);
        }

        public async Task ValidateProduct(UpsertProductDTO product)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                throw new ValidationException("Product name is required.");
            }

            var categories = await categoryRepository.GetAllAsync();
            var categoryIds = categories.Select(c => c.Id).ToList();

            if (!categoryIds.Contains(product.CategoryId))
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
