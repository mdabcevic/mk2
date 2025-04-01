using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Bartender.Domain.Services;

public class ProductService(
    IRepository<Products> repository, 
    IRepository<ProductCategory> categoryRepository,
    ILogger<ProductService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper) : IProductService
{
    private const string GenericErrorMessage = "An unexpected error occurred. Please try again later.";
    public async Task<ServiceResult<ProductDto?>> GetByIdAsync(int id)
    {
        try
        {
            var user = await currentUser.GetCurrentUserAsync();
            var product = await repository.GetByIdAsync(id, true);

            if (product == null)
                return ServiceResult<ProductDto?>.Fail($"Product with id {id} not found", ErrorType.NotFound);

            if (!VerifyProductAccess(user!, product.BusinessId, false))
            {
                logger.LogWarning($"Access denied: User {user.Id} (Business: {user.Place!.BusinessId}) attempted to access product from Business {product.BusinessId}.");
                return ServiceResult<ProductDto?>.Fail($"Cross-business access denied.", ErrorType.NotFound);
            }

            var dto = mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto?>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the product.");
            return ServiceResult<ProductDto?>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<ProductDto>>> GetAllAsync(bool? exclusive = null)
    {
        try
        {
            var user = await currentUser.GetCurrentUserAsync();

            Expression<Func<Products, bool>>? filter = null;

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
            return ServiceResult<List<ProductDto>>.Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the products.");
            return ServiceResult<List<ProductDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<GroupedProductsDto>>> GetAllGroupedAsync(bool? exclusive = null)
    {
        try
        {
            var user = await currentUser.GetCurrentUserAsync();

            var groupedProducts = await categoryRepository.QueryIncluding()
                .Select(g => new GroupedProductsDto
                {
                    Category = g.Name,
                    Products = g.Products
                        .Where(p =>
                            (exclusive == null && user!.Role == EmployeeRole.admin) ||
                            (exclusive == null && (p.BusinessId == user.Place!.BusinessId || p.BusinessId == null)) ||
                            (exclusive == true && user!.Role == EmployeeRole.admin && p.BusinessId != null) ||
                            (exclusive == true && p.BusinessId == user.Place!.BusinessId) ||
                            (exclusive == false && p.BusinessId == null)
                        )
                        .OrderBy(p => p.Name)
                        .Select(p => mapper.Map<ProductBaseDto>(p))
                        .ToList()
                })
                .Where(g => g.Products.Any())
                .ToListAsync();

            return ServiceResult<List<GroupedProductsDto>>.Ok(groupedProducts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching the products.");
            return ServiceResult<List<GroupedProductsDto>>.Fail(GenericErrorMessage, ErrorType.Unknown);
        }
    }

    public async Task<ServiceResult<List<ProductBaseDto>>> GetFilteredAsync(bool? exclusive = null, string? name = null, string? category = null)
    {
        try
        {
            var user = await currentUser.GetCurrentUserAsync();

            var query = repository.QueryIncluding(p => p.Category);


            Expression<Func<Products, bool>>? filter = null;

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
            var user = await currentUser.GetCurrentUserAsync();

            if (user!.Role != EmployeeRole.admin)
                product.BusinessId = user!.Place!.BusinessId;

            var validationResult = await ValidateProductAsync(product);

            if (!validationResult.Success)
                return validationResult;

            var newProduct = mapper.Map<Products>(product);

            await repository.AddAsync(newProduct);
            logger.LogInformation($"Product created: {product.Name} {product.Volume}, BusinessId: {product.BusinessId}");
            return ServiceResult.Ok();
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
            var user = await currentUser.GetCurrentUserAsync();

            var updateProduct = await repository.GetByIdAsync(id);
            if (updateProduct == null)
                return ServiceResult.Fail($"Product with id {id} not found", ErrorType.NotFound);

            if (product.BusinessId != updateProduct.BusinessId && user!.Role != EmployeeRole.admin)
                product.BusinessId = updateProduct.BusinessId;

            if (!VerifyProductAccess(user!, product.BusinessId, true))
            {
                logger.LogWarning($"Access denied: User {user.Id} (Business: {user.Place!.BusinessId}) attempted to update product from Business {product.BusinessId}.");
                return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);
            }

            var validationResult = await ValidateProductAsync(product,id);

            if (!validationResult.Success)
                return validationResult;
    

            mapper.Map(product, updateProduct);

            await repository.UpdateAsync(updateProduct);
            logger.LogInformation($"Product updated with ID: {id}");
            return ServiceResult.Ok();
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
            var product = await repository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult.Fail($"Product with id {id} not found", ErrorType.NotFound);

            var user = await currentUser.GetCurrentUserAsync();
            if (user!.Role != EmployeeRole.admin && product.BusinessId != user!.Place!.BusinessId)
            {
                logger.LogWarning($"Access denied: User {user.Id} (Business: {user.Place!.BusinessId}) attempted to delete product from Business {product.BusinessId}.");
                return ServiceResult.Fail("Product can only be deleted by owning business or administrators.", ErrorType.Unknown);
            }

            await repository.DeleteAsync(product);
            logger.LogInformation($"Product deleted with ID: {id}");
            return ServiceResult.Ok();
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

    private async Task<ServiceResult> ValidateProductAsync(UpsertProductDto product, int? id = null)
    {
        bool categoryExists = await categoryRepository.ExistsAsync(c => c.Id == product.CategoryId);
        if (!categoryExists)
            return ServiceResult.Fail($"Product category id {product.CategoryId} doesn't exist", ErrorType.Validation);

        var existingProduct = await repository.ExistsAsync(p =>
            (id == null || p.Id != id) &&
            (p.BusinessId == null || p.BusinessId == product.BusinessId) &&
            p.Name.ToLower() == product.Name.ToLower() &&
            (p.Volume == null && product.Volume == null ||
             p.Volume != null && product.Volume != null && p.Volume.ToLower() == product.Volume.ToLower()));

        if (existingProduct)
            return ServiceResult.Fail($"Product with name '{product.Name}' and volume '{product.Volume}' already exists.", ErrorType.Conflict);

        return ServiceResult.Ok();
    }

    private bool VerifyProductAccess(Staff user, int? businessId, bool upsert)
    {
        if (user.Role == EmployeeRole.admin)
            return true;

        if (businessId == null && !upsert)
            return true;

        return user!.Place!.BusinessId == businessId;
    }
}
