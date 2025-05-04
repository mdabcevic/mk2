using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
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
    public async Task AddAsync_ValidProduct_ReturnsSuccess()
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

        // Act
        var result = await _productService.AddAsync(dto);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).AddAsync(product);
    }

    [Test]
    public async Task AddAsync_InvalidCategory_ReturnsValidationFailure()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(categoryId: 999);
        var staff = TestDataFactory.CreateValidStaff();
        _currentUser.GetCurrentUserAsync().Returns(staff);

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(false);

        // Act
        var result = await _productService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Validation));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Test]
    public async Task AddAsync_DuplicateProduct_ReturnsConflictFailure()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto();
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true);

        // Act
        var result = await _productService.AddAsync(dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    // UpdateAsync tests go here

    [Test]
    public async Task UpdateAsync_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(name: "Updated", volume: "2L", categoryId: 1);
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act
        var result = await _productService.UpdateAsync(1, dto);

        // Assert
        Assert.That(result.Success, Is.True);
        _mapper.Received(1).Map(dto, existing);
        await _repository.Received(1).UpdateAsync(existing);
    }

    [Test]
    public async Task UpdateAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto();
        var staff = TestDataFactory.CreateValidStaff();

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(999).Returns((Product?)null);

        // Act
        var result = await _productService.UpdateAsync(999, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    [Test]
    public async Task UpdateAsync_CrossBusinessDenied_ReturnsUnauthorized()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(businessId: 2);
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 2);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act
        var result = await _productService.UpdateAsync(1, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unauthorized));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    [Test]
    public async Task UpdateAsync_DuplicateProduct_ReturnsConflict()
    {
        // Arrange
        var dto = TestDataFactory.CreateValidUpsertProductDto(name: "Duplicate", volume: "1L");
        var existing = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1).Returns(existing);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>()).Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true); // name+volume already taken

        // Act
        var result = await _productService.UpdateAsync(1, dto);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Conflict));
        });
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>());
        _mapper.DidNotReceive().Map(Arg.Any<UpsertProductDto>(), Arg.Any<Product>());
    }

    // DeleteAsync tests go here

    [Test]
    public async Task DeleteAsync_ValidDelete_ReturnsSuccess()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(1, businessId: 1);
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _repository.GetByIdAsync(1).Returns(product);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _productService.DeleteAsync(1);

        // Assert
        Assert.That(result.Success, Is.True);
        await _repository.Received(1).DeleteAsync(product);
    }

    [Test]
    public async Task DeleteAsync_ProductNotFound_ReturnsNotFound()
    {
        // Arrange
        _repository.GetByIdAsync(999).Returns((Product?)null);

        // Act
        var result = await _productService.DeleteAsync(999);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        });
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Product>());
    }

    [Test]
    public async Task DeleteAsync_CrossBusinessDenied_ReturnsFailure()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(1, businessId: 2); // foreign business
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);

        _repository.GetByIdAsync(1).Returns(product);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        // Act
        var result = await _productService.DeleteAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.Unknown));
        });
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Product>());
    }
}
