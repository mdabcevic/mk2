using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Bartender.Domain.Utility.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;

namespace BartenderTests;

[TestFixture]
public class ProductServiceMutationTests
{
    private IRepository<Product> _repository;
    private IRepository<ProductCategory> _categoryRepository;
    private ICurrentUserContext _currentUser;
    private IMapper _mapper;
    private ILogger<ProductService> _logger;
    private ProductService _productService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Product>>();
        _categoryRepository = Substitute.For<IRepository<ProductCategory>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<ProductService>>();
        _productService = new ProductService(_repository, _categoryRepository, _logger, _currentUser, _mapper);
    }

    // AddAsync tests

    [Test]
    public async Task AddAsync_ValidProduct_ShouldAddSuccessfully()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto();
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        var product = TestDataFactory.CreateMappedProductFromDto(dto);
        product.BusinessId = 1;

        _mapper.Map<Product>(dto).Returns(product);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _productService.AddAsync(dto));
        await _repository.Received(1).AddAsync(product);
    }

    [Test]
    public async Task AddAsync_ShouldThrow_WhenCategoryDoesNotExist()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(categoryId: 999);
        var staff = TestDataFactory.CreateValidStaff();

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _productService.AddAsync(dto));

        Assert.That(ex.Message, Is.EqualTo("Product category id 999 not found"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Test]
    public async Task AddAsync_ShouldThrow_WhenDuplicateProductExists()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto();
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true); // duplicate

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await _productService.AddAsync(dto));

        Assert.That(ex.Message, Does.Contain($"Product with name '{dto.Name}'"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    // UpdateAsync tests go here

    [Test]
    public async Task UpdateAsync_ShouldUpdate_WhenValid()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(name: "Updated", volume: "2L", categoryId: 1);
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _productService.UpdateAsync(1, dto));

        _mapper.Received(1).Map(dto, existing);
        await _repository.Received(1).UpdateAsync(existing);
    }

    [Test]
    public async Task UpdateAsync_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto();
        var staff = TestDataFactory.CreateValidStaff();

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(999).Returns((Product?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ProductNotFoundException>(async () =>
            await _productService.UpdateAsync(999, dto));

        Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    [Test]
    public async Task UpdateAsync_ShouldThrow_WhenCrossBusinessAccessDenied()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(businessId: 2);
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 2);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1); // different business

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(async () =>
            await _productService.UpdateAsync(1, dto));

        Assert.That(ex.Message, Is.EqualTo("Access to product denied"));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    [Test]
    public async Task UpdateAsync_ShouldThrow_WhenDuplicateProductExists()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(name: "Duplicate", volume: "1L");
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true); // duplicate

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await _productService.UpdateAsync(1, dto));

        Assert.That(ex.Message, Does.Contain($"Product with name '{dto.Name}'"));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    // DeleteAsync tests go here

    [Test]
    public async Task DeleteAsync_ShouldSucceed_WhenProductIsValidAndAuthorized()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _repository.GetByIdAsync(1).Returns(product);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _productService.DeleteAsync(1));
        await _repository.Received(1).DeleteAsync(product);
    }

    [Test]
    public async Task DeleteAsync_ShouldThrow_WhenProductNotFound()
    {
        // Arrange
        _repository.GetByIdAsync(999).Returns((Product?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ProductNotFoundException>(async () =>
            await _productService.DeleteAsync(999));

        Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Product>());
    }

    [Test]
    public async Task DeleteAsync_ShouldThrow_WhenAccessingProductFromOtherBusiness()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(1, businessId: 2); // foreign business
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _repository.GetByIdAsync(1).Returns(product);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(async () =>
            await _productService.DeleteAsync(1));

        Assert.That(ex.Message, Is.EqualTo("Access to product denied"));
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Product>());
    }
}
