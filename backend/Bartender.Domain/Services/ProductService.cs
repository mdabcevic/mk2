using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bartender.Domain.Services;

public class ProductService(
    IRepository<Products> repository, 
    IRepository<ProductCategory> categoryRepository,
    ILogger<ProductService> logger,
    IMapper mapper) : IProductService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again later.";
    public async Task<ServiceResult<ProductDto?>> GetByIdAsync(int id)
    {
        try
        {
            var product = await repository.GetByIdAsync(id, true);
            if (product == null)
                return ServiceResult<ProductDto?>.Fail($"Product with id {id} not found", ErrorType.NotFound);

            var dto = mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto?>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the product.");
            return ServiceResult<ProductDto?>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await repository.GetAllAsync(true, p => p.Name);

            if (!products.Any())
                return ServiceResult<List<ProductDto>>.Fail("There are currently no products", ErrorType.NotFound);

            var dto = mapper.Map<List<ProductDto>>(products);
            return ServiceResult<List<ProductDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the products.");
            return ServiceResult<List<ProductDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<GroupedProductsDto>>> GetAllGroupedAsync()
    {
        try
        {
            var groupedProducts = await categoryRepository.GetAllAsync(true);

            if (!groupedProducts.Any())
                return ServiceResult<List<GroupedProductsDto>>.Fail("There are currently no products", ErrorType.NotFound);

            var sortedProducts = groupedProducts
                .Select(gp => new GroupedProductsDto
                {
                    Category = gp.Name ?? "Uncategorized",
                    Products = gp.Products != null
                    ? gp.Products
                        .OrderBy(p => p.Name)
                        .Select(p => mapper.Map<ProductBaseDto>(p))
                        .ToList()
                    : new List<ProductBaseDto>()
                })
                .ToList();

            return ServiceResult<List<GroupedProductsDto>>.Ok(sortedProducts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the products.");
            return ServiceResult<List<GroupedProductsDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<ProductBaseDto>>> GetFilteredAsync(string? name = null, string? category = null)
    {
        try
        {
            var query = repository.QueryIncluding(p => p.Category);

            if (name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{name}%"));

            if (category != null)
                query = query.Where(x => EF.Functions.ILike(x.Category.Name, $"%{category}%"));

            var products = await query.ToListAsync();

            if (!products.Any())
                return ServiceResult<List<ProductBaseDto>>.Fail("No products found matching the criteria", ErrorType.NotFound);

            var dto = mapper.Map<List<ProductBaseDto>>(products);
            return ServiceResult<List<ProductBaseDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the products.");
            return ServiceResult<List<ProductBaseDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> AddAsync(UpsertProductDto product)
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
            logger.LogError(ex, "An unexpected error occurred while adding a product.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertProductDto product)
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
            logger.LogError(ex, "An unexpected error occurred while updating a product.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while deleting a product.");
            return ServiceResult.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<ProductCategoryDto>>> GetProductCategoriesAsync()
    {
        try
        {
            var categories = await categoryRepository.GetAllAsync();
            var dto = mapper.Map<List<ProductCategoryDto>>(categories);
            return ServiceResult<List<ProductCategoryDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching product categories.");
            return ServiceResult<List<ProductCategoryDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    private async Task ValidateProductAsync(UpsertProductDto product)
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
