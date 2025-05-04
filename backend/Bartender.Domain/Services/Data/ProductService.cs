using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Bartender.Domain.utility.Exceptions;

namespace Bartender.Domain.Services.Data;

public class ProductService(
    IRepository<Product> repository, 
    IRepository<ProductCategory> categoryRepository,
    ILogger<ProductService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper) : IProductService
{
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var user = await currentUser.GetCurrentUserAsync();
        var product = await repository.GetByIdAsync(id, true);

        if (product == null)
            throw new ProductNotFoundException(id);

        if (!VerifyProductAccess(user!, product.BusinessId, false))
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

        Expression<Func<Product, bool>>? filter = null;

        if (user!.Role != EmployeeRole.admin && exclusive == null)
            filter = p => p.BusinessId == user.Place!.BusinessId || p.BusinessId == null;

        else if (exclusive == true)
        {
            if (user!.Role == EmployeeRole.admin)
                filter = p => p.BusinessId != null;
            else
                filter = p => p.BusinessId == user.Place!.BusinessId;
        }

        else if (exclusive == false)
            filter = p => p.BusinessId == null;    

        var products = await repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: filter,
            orderBy: p => p.Name);

        var dto = mapper.Map<List<ProductDto>>(products);
        return dto;
    }

    public async Task<List<GroupedProductsDto>> GetAllGroupedAsync(bool? exclusive = null)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var groupedProducts = await categoryRepository.QueryIncluding()
            .Select(g => new GroupedProductsDto
            {
                Category = g.Name,
                Products = g.Products
                    .Where(p =>
                        exclusive == null && user!.Role == EmployeeRole.admin ||
                        exclusive == null && (p.BusinessId == user.Place!.BusinessId || p.BusinessId == null) ||
                        exclusive == true && user!.Role == EmployeeRole.admin && p.BusinessId != null ||
                        exclusive == true && p.BusinessId == user.Place!.BusinessId ||
                        exclusive == false && p.BusinessId == null
                    )
                    .OrderBy(p => p.Name)
                    .Select(p => mapper.Map<ProductBaseDto>(p))
                    .ToList()
            })
            .Where(g => g.Products.Any())
            .ToListAsync();

        return groupedProducts;
    }

    public async Task<List<ProductBaseDto>> GetFilteredAsync(bool? exclusive = null, string? name = null, string? category = null)
    {
        var user = await currentUser.GetCurrentUserAsync();

        var query = repository.QueryIncluding(p => p.Category);


        Expression<Func<Product, bool>>? filter = null;

        if (exclusive == null && user!.Role != EmployeeRole.admin)
            filter = p => p.BusinessId == user.Place!.BusinessId || p.BusinessId == null;

        else if (exclusive == true)
        {
            if (user!.Role == EmployeeRole.admin)
                filter = p => p.BusinessId != null;
            else
                filter = p => p.BusinessId == user.Place!.BusinessId;
        }

        else if (exclusive == false)
            filter = p => p.BusinessId == null;

        if (filter != null)
            query = query.Where(filter);

        query = query.Where(p =>
            (name == null || EF.Functions.ILike(p.Name, $"%{name}%")) &&
            (category == null || EF.Functions.ILike(p.Category.Name, $"%{category}%")));

        var products = await query.ToListAsync();

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

        if (!VerifyProductAccess(user!, product.BusinessId, true))
        {
            throw new AuthorizationException("Access to product denied")
                .WithLogMessage($"Access denied: User {user!.Id} (Business: {user.Place!.BusinessId}) attempted to update product from Business {product.BusinessId}.");
        }

        await ValidateProductAsync(product,id);  

        mapper.Map(product, updateProduct);

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

        await repository.DeleteAsync(product);
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

        var existingProduct = await repository.ExistsAsync(p =>
            (id == null || p.Id != id) &&
            (p.BusinessId == null || p.BusinessId == product.BusinessId) &&
            p.Name.ToLower() == product.Name.ToLower() &&
            (p.Volume == null && product.Volume == null ||
             p.Volume != null && product.Volume != null && p.Volume.ToLower() == product.Volume.ToLower()));

        if (existingProduct)
            throw new ConflictException($"Product with name '{product.Name}' and volume '{product.Volume}' already exists.");
    }

    private static bool VerifyProductAccess(Staff user, int? businessId, bool upsert)
    {
        if (user.Role == EmployeeRole.admin)
            return true;

        if (businessId == null && !upsert)
            return true;

        return user!.Place!.BusinessId == businessId;
    }
}
