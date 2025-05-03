using AutoMapper;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BartenderTests;
[TestFixture]
public class ProductsServiceTests
{
    private IRepository<Product> _repository;
    private IRepository<ProductCategory> _categoryRepository;
    private IMapper _mapper;
    private ILogger<ProductService> _logger;
    private ICurrentUserContext _currentUser;
    private ProductService _productService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Product>>();
        _categoryRepository = Substitute.For<IRepository<ProductCategory>>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<ProductService>>();
        _currentUser = Substitute.For<ICurrentUserContext>();
        _productService = new ProductService(_repository, _categoryRepository, _logger, _currentUser, _mapper);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct();
        var productDto = TestDataFactory.CreateValidProductDto();

        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        _repository.GetByIdAsync(1, true).Returns(product);
        _mapper.Map<ProductDto>(product).Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data!.Name, Is.EqualTo("Espresso"));
            Assert.That(result.Data!.Volume, Is.EqualTo("ŠAL"));
            Assert.That(result.Data!.Category!.Id, Is.EqualTo(2));
            Assert.That(result.Data!.Category.Name, Is.EqualTo("Coffee"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1, true).Returns((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
        Assert.That(result.Error, Does.Contain("Product with id 1 not found"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNotFound_WhenAccessingOtherBusinessProduct()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(businessId: 99); // Different business
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1, true).Returns(product);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.errorType, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error, Is.EqualTo("Cross-business access denied."));
        });
    }

    [Test]
    public async Task GetAllAsync_Default_ReturnsExpectedProducts()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var products = new List<Product>
        {
            TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Product 1", volume: "0.2L"),
            TestDataFactory.CreateSharedProduct(2, name: "Product 2", volume: "0.3L")
        };

        var productDtos = new List<ProductDto>
        {
            TestDataFactory.CreateValidProductDto(1, name: "Product 1", volume: "0.2L"),
            TestDataFactory.CreateValidProductDto(2, name: "Product 2", volume: "0.3L")
        };

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderByDescending: false,
            Arg.Any<Expression<Func<Product, object>>[]>()
            )
            .Returns(products);

        _mapper.Map<List<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetAllAsync_AdminExclusiveTrue_ReturnsExclusiveProducts()
    {
        // Arrange
        var admin = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(admin);

        var exclusiveProduct = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Exclusive");

        _repository.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<Product, bool>>>(),
            false,
            Arg.Any<Expression<Func<Product, object>>[]>()
        ).Returns([exclusiveProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>())
            .Returns([TestDataFactory.CreateValidProductDto(1, name: "Exclusive")]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });
        Assert.That(result.Data![0].Name, Is.EqualTo("Exclusive"));
    }

    [Test]
    public async Task GetAllAsync_RegularExclusiveTrue_ReturnsBusinessOnly()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var businessProduct = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "BizProd");

        _repository.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<Product, bool>>>(),
            false,
            Arg.Any<Expression<Func<Product, object>>[]>()
        ).Returns([businessProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>())
            .Returns([TestDataFactory.CreateValidProductDto(1, name: "BizProd")]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });
        Assert.That(result.Data![0].Name, Is.EqualTo("BizProd"));
    }

    [Test]
    public async Task GetAllAsync_ExclusiveFalse_ReturnsSharedProductsOnly()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var sharedProduct = TestDataFactory.CreateSharedProduct(2, name: "SharedOnly");

        _repository.GetFilteredAsync(
            true,
            Arg.Any<Expression<Func<Product, bool>>>(),
            false,
            Arg.Any<Expression<Func<Product, object>>[]>()
        ).Returns([sharedProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>())
            .Returns([TestDataFactory.CreateValidProductDto(2, name: "SharedOnly")]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });
        Assert.That(result.Data![0].Name, Is.EqualTo("SharedOnly"));
    }
    }
}

