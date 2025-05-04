using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.Utility.Exceptions;

namespace Bartender.Domain.Services.Data;

public class ProductService(
    IProductRepository repository, 
    IRepository<ProductCategory> categoryRepository,
    ILogger<ProductService> logger,
    ICurrentUserContext currentUser,
    IValidationService validationService,
    IMapper mapper) : IProductService
{
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var product = await repository.GetByIdAsync(id, true);

        if (product == null)
            throw new ProductNotFoundException(id);

        if (!await validationService.VerifyProductAccess(product.BusinessId, false, user))
        {
            throw new AuthorizationException("Access to product denied")
                .WithLogMessage($"Access denied: User {user!.Id} (Business: {user.Place!.BusinessId}) attempted to access product from Business {product.BusinessId}.");
        }

        var dto = mapper.Map<ProductDto>(product);
        return dto;
    }

    public async Task<List<ProductDto>> GetAllAsync(bool? exclusive = null)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var products = await repository.GetAllProductsAsync(
            businessId: user.Place?.BusinessId,
            exclusive: exclusive,
            isAdmin: user.Role == EmployeeRole.admin);

        var dto = mapper.Map<List<ProductDto>>(products);
        return dto;
    }

    public async Task<List<GroupedProductsDto>> GetAllGroupedAsync(bool? exclusive = null)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var groupedProducts = await repository.GetProductsGroupedAsync(
            businessId: user.Place?.BusinessId,
            exclusive: exclusive,
            isAdmin: user.Role == EmployeeRole.admin);

        var result = groupedProducts.Select(g => new GroupedProductsDto
        {
            Category = g.Key.Name,
            Products = mapper.Map<List<ProductBaseDto>>(g.Value)
        }).ToList();

        return result; 
    }

    public async Task<List<ProductBaseDto>> GetFilteredAsync(bool? exclusive = null, string? name = null, string? category = null)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var products = await repository.GetProductsFilteredAsync(
            exclusive: exclusive,
            isAdmin: user!.Role == EmployeeRole.admin,
            name: name,
            category: category);

        var dto = mapper.Map<List<ProductBaseDto>>(products);
        return dto;
    }

    public async Task AddAsync(UpsertProductDto product)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user!.Role != EmployeeRole.admin)
            product.BusinessId = user!.Place!.BusinessId;

        await ValidateProductAsync(product);

        var newProduct = mapper.Map<Product>(product);

        await repository.AddAsync(newProduct);
        logger.LogInformation(
            "Product created: {ProductName} {ProductVolume}, BusinessId: {BusinessId}", product.Name, product.Volume, product.BusinessId);
    }

    public async Task UpdateAsync(int id, UpsertProductDto product)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var updateProduct = await repository.GetByIdAsync(id);
        if (updateProduct == null)
            throw new ProductNotFoundException(id);

        if (product.BusinessId != updateProduct.BusinessId && user!.Role != EmployeeRole.admin)
            product.BusinessId = updateProduct.BusinessId;

        if (!await validationService.VerifyProductAccess(product.BusinessId, true, user))
        {
            throw new AuthorizationException("Access to product denied")
                .WithLogMessage($"Access denied: User {user!.Id} (Business: {user.Place!.BusinessId}) attempted to update product from Business {product.BusinessId}.");
        }

        await ValidateProductAsync(product,id);  

        mapper.Map(product, updateProduct);

        updateProduct.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(updateProduct);
        logger.LogInformation("Product updated with ID: {ProductId}", id);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        var user = await currentUser.GetCurrentUserAsync();
        if (user!.Role != EmployeeRole.admin && product.BusinessId != user!.Place!.BusinessId)
        {
            throw new AuthorizationException("Access to product denied")
                .WithLogMessage($"Access denied: User {user!.Id} (Business: {user.Place!.BusinessId}) attempted to delete product from Business {product.BusinessId}.");
        }

        product.DeletedAt = DateTime.UtcNow;
        await repository.UpdateAsync(product);
        logger.LogInformation("Product deleted with ID: {ProductId}", id);
    }

    public async Task<List<ProductCategoryDto>> GetProductCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        var dto = mapper.Map<List<ProductCategoryDto>>(categories);
        return dto;
    }

    private async Task ValidateProductAsync(UpsertProductDto product, int? id = null)
    {
        bool categoryExists = await categoryRepository.ExistsAsync(c => c.Id == product.CategoryId);
        if (!categoryExists)
            throw new NotFoundException($"Product category id {product.CategoryId} not found");

        var existingProduct = await repository.ProductExists(
            productId: id,
            businessId: product.BusinessId,
            name: product.Name,
            volume: product.Volume
         );

        if (existingProduct)
            throw new ConflictException($"Product with name '{product.Name}' and volume '{product.Volume}' already exists.");
    }
}
