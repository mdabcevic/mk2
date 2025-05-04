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
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace BartenderTests;
[TestFixture]
public class ProductServiceReadTests
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
        var product = TestDataFactory.CreateValidProduct();  // domain model
        var productDto = TestDataFactory.CreateValidProductDto();  // expected DTO

        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1, true).Returns(product);
        _mapper.Map<ProductDto>(product).Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Espresso"));
            Assert.That(result.Volume, Is.EqualTo("ŠAL"));
            Assert.That(result.Category, Is.Not.Null);
            Assert.That(result.Category!.Id, Is.EqualTo(2));
            Assert.That(result.Category.Name, Is.EqualTo("Coffee"));
        });
    }

    [Test]
    public void GetByIdAsync_ShouldThrow_WhenProductDoesNotExist()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1, true).Returns((Product?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ProductNotFoundException>(async () =>
            await _productService.GetByIdAsync(1));

        Assert.That(ex.Message, Is.EqualTo("Product with id 1 not found"));
        _mapper.DidNotReceive().Map<ProductDto>(Arg.Any<Product>());
    }

    [Test]
    public void GetByIdAsync_ShouldThrow_WhenAccessingOtherBusinessProduct()
    {
        // Arrange
        var product = TestDataFactory.CreateValidProduct(businessId: 99); // Product from a different business
        var staff = TestDataFactory.CreateValidStaff(businessid: 1);      // User from business 1

        _currentUser.GetCurrentUserAsync().Returns(staff);
        _repository.GetByIdAsync(1, true).Returns(product);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AuthorizationException>(async () =>
            await _productService.GetByIdAsync(1));

        Assert.That(ex.Message, Is.EqualTo("Access to product denied"));
        _mapper.DidNotReceive().Map<ProductDto>(Arg.Any<Product>());
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
        TestDataFactory.CreateValidProduct(2, name: "Product 2", volume: "0.3L") // shared product
    };

        var productDtos = new List<ProductDto>
    {
        TestDataFactory.CreateValidProductDto(1, name: "Product 1", volume: "0.2L"),
        TestDataFactory.CreateValidProductDto(2, name: "Product 2", volume: "0.3L")
    };

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        ).Returns(products);

        _mapper.Map<List<ProductDto>>(products).Returns(productDtos);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Product 1"));
            Assert.That(result[1].Name, Is.EqualTo("Product 2"));
        });

        _mapper.Received(1).Map<List<ProductDto>>(products);
    }

    [Test]
    public async Task GetAllAsync_AdminExclusiveTrue_ReturnsExclusiveProducts()
    {
        // Arrange
        var admin = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
        _currentUser.GetCurrentUserAsync().Returns(admin);

        var exclusiveProduct = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Exclusive");
        var exclusiveDto = TestDataFactory.CreateValidProductDto(1, name: "Exclusive");

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        ).Returns([exclusiveProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>()).Returns([exclusiveDto]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Exclusive"));
        });

        _mapper.Received(1).Map<List<ProductDto>>(Arg.Any<List<Product>>());
    }

    [Test]
    public async Task GetAllAsync_RegularExclusiveTrue_ReturnsBusinessOnly()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var businessProduct = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "BizProd");
        var businessProductDto = TestDataFactory.CreateValidProductDto(1, name: "BizProd");

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        ).Returns([businessProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>())
            .Returns([businessProductDto]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: true);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("BizProd"));
        });

        _mapper.Received(1).Map<List<ProductDto>>(Arg.Any<List<Product>>());
    }

    [Test]
    public async Task GetAllAsync_ExclusiveFalse_ReturnsSharedProductsOnly()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var sharedProduct = TestDataFactory.CreateValidProduct(2, name: "SharedOnly");
        var sharedProductDto = TestDataFactory.CreateValidProductDto(2, name: "SharedOnly");

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        ).Returns([sharedProduct]);

        _mapper.Map<List<ProductDto>>(Arg.Any<List<Product>>())
            .Returns([sharedProductDto]);

        // Act
        var result = await _productService.GetAllAsync(exclusive: false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("SharedOnly"));
        });

        _mapper.Received(1).Map<List<ProductDto>>(Arg.Any<List<Product>>());
    }

    [Test]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoProductsExist()
    {
        // Arrange
        var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
        _currentUser.GetCurrentUserAsync().Returns(staff);

        var emptyList = new List<Product>();

        _repository.GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        ).Returns(emptyList);

        _mapper.Map<List<ProductDto>>(emptyList).Returns(new List<ProductDto>());

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);

        await _repository.Received(1).GetFilteredAsync(
            includeNavigations: true,
            filterBy: Arg.Any<Expression<Func<Product, bool>>>(),
            orderBy: Arg.Any<Expression<Func<Product, object>>>()
        );

        _mapper.Received(1).Map<List<ProductDto>>(emptyList);
    }

    [Test]
    public async Task GetProductCategoriesAsync_ReturnsMappedCategoryList()
    {
        // Arrange
        var categories = new List<ProductCategory>
    {
        TestDataFactory.CreateValidProductCategory(1, "Hot Drinks"),
        TestDataFactory.CreateValidProductCategory(2, "Cold Drinks")
    };

        var dtoList = new List<ProductCategoryDto>
    {
        new() { Id = 1, Name = "Hot Drinks" },
        new() { Id = 2, Name = "Cold Drinks" }
    };

        _categoryRepository.GetAllAsync().Returns(categories);
        _mapper.Map<List<ProductCategoryDto>>(categories).Returns(dtoList);

        // Act
        var result = await _productService.GetProductCategoriesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Hot Drinks"));
            Assert.That(result[1].Name, Is.EqualTo("Cold Drinks"));
        });

        await _categoryRepository.Received(1).GetAllAsync();
        _mapper.Received(1).Map<List<ProductCategoryDto>>(categories);
    }

    [Test]
    public void GetProductCategoriesAsync_ShouldThrow_WhenExceptionThrown()
    {
        // Arrange
        _categoryRepository.GetAllAsync().Throws(new Exception("DB error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _productService.GetProductCategoriesAsync());

        Assert.That(ex.Message, Is.EqualTo("DB error"));
    }

    //[Test]
    //public async Task GetAllGroupedAsync_RegularNull_ReturnsOwnAndSharedProducts()
    //{
    //    // Arrange
    //    var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
    //    _currentUser.GetCurrentUserAsync().Returns(staff);

    //    var matchingProduct1 = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Espresso");
    //    var matchingProduct2 = TestDataFactory.CreateSharedProduct(2, name: "Americano");

    //    var category = new ProductCategory
    //    {
    //        Id = 10,
    //        Name = "Coffee",
    //        Products = [matchingProduct1, matchingProduct2]
    //    };

    //    _categoryRepository.QueryIncluding().Returns(new List<ProductCategory> { category }.AsQueryable());

    //    _mapper.Map<ProductBaseDto>(matchingProduct1)
    //        .Returns(TestDataFactory.CreateProductBaseDto(1, "Espresso", "S"));

    //    _mapper.Map<ProductBaseDto>(matchingProduct2)
    //        .Returns(TestDataFactory.CreateProductBaseDto(2, "Americano", "M"));

    //    // Act
    //    var result = await _productService.GetAllGroupedAsync();

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data, Has.Count.EqualTo(1));
    //    });
    //    var group = result.Data!.First();
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(group.Category, Is.EqualTo("Coffee"));
    //        Assert.That(group.Products.Count, Is.EqualTo(2));
    //    });
    //}

    //[Test]
    //public async Task GetAllGroupedAsync_AdminNull_ReturnsAllProducts()
    //{
    //    // Arrange
    //    var admin = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
    //    _currentUser.GetCurrentUserAsync().Returns(admin);

    //    var prod1 = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Espresso");
    //    var prod2 = TestDataFactory.CreateSharedProduct(2, name: "Macchiato");

    //    var category = TestDataFactory.CreateValidProductCategory(2, "AdminCat", [prod1, prod2]);

    //    _categoryRepository.QueryIncluding().Returns(new List<ProductCategory> { category }.AsQueryable());

    //    _mapper.Map<ProductBaseDto>(prod1).Returns(TestDataFactory.CreateProductBaseDto(1, "Espresso"));
    //    _mapper.Map<ProductBaseDto>(prod2).Returns(TestDataFactory.CreateProductBaseDto(2, "Macchiato"));

    //    // Act
    //    var result = await _productService.GetAllGroupedAsync();

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data!.First().Products, Has.Count.EqualTo(2));
    //    });
    //}

    //[Test]
    //public async Task GetAllGroupedAsync_AdminExclusiveTrue_ReturnsOnlyExclusive()
    //{
    //    // Arrange
    //    var admin = TestDataFactory.CreateValidStaff(role: EmployeeRole.admin);
    //    _currentUser.GetCurrentUserAsync().Returns(admin);

    //    var exclusive = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "AdminExclusive");
    //    var category = TestDataFactory.CreateValidProductCategory(2, "AdminCat", [exclusive]);

    //    _categoryRepository.QueryIncluding().Returns(new List<ProductCategory> { category }.AsQueryable());

    //    _mapper.Map<ProductBaseDto>(exclusive).Returns(TestDataFactory.CreateProductBaseDto(1, "AdminExclusive"));

    //    // Act
    //    var result = await _productService.GetAllGroupedAsync(exclusive: true);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data!, Has.Count.EqualTo(1));
    //        Assert.That(result.Data!.First().Products, Has.Count.EqualTo(1));
    //        Assert.That(result.Data!.First().Products!.First().Name, Is.EqualTo("AdminExclusive"));
    //    });
    //}

    //[Test]
    //public async Task GetAllGroupedAsync_RegularExclusiveTrue_ReturnsOnlyBusinessProducts()
    //{
    //    // Arrange
    //    var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
    //    _currentUser.GetCurrentUserAsync().Returns(staff);

    //    var ownProduct = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "MyBizItem");
    //    var foreignProduct = TestDataFactory.CreateValidProduct(2, businessId: 999, name: "OtherBizItem");

    //    var category = TestDataFactory.CreateValidProductCategory(3, "RegularCat", [ownProduct, foreignProduct]);

    //    _categoryRepository.QueryIncluding().Returns(new List<ProductCategory> { category }.AsQueryable());

    //    _mapper.Map<ProductBaseDto>(ownProduct).Returns(TestDataFactory.CreateProductBaseDto(1, "MyBizItem"));
    //    // Do not map foreignProduct – it's filtered out

    //    // Act
    //    var result = await _productService.GetAllGroupedAsync(exclusive: true);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data!.Count, Is.EqualTo(1));
    //        Assert.That(result.Data.First().Products.Count, Is.EqualTo(1));
    //        Assert.That(result.Data.First().Products.First().Name, Is.EqualTo("MyBizItem"));
    //    });
    //}

    //[Test]
    //public async Task GetAllGroupedAsync_ExclusiveFalse_ReturnsOnlySharedProducts()
    //{
    //    // Arrange
    //    var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
    //    _currentUser.GetCurrentUserAsync().Returns(staff);

    //    var shared = TestDataFactory.CreateSharedProduct(1, name: "Shared");
    //    var businessOwned = TestDataFactory.CreateValidProduct(2, businessId: 1, name: "Owned");

    //    var category = TestDataFactory.CreateValidProductCategory(4, "SharedCat", [shared, businessOwned]);

    //    _categoryRepository.QueryIncluding().Returns(new List<ProductCategory> { category }.AsQueryable());

    //    _mapper.Map<ProductBaseDto>(shared).Returns(TestDataFactory.CreateProductBaseDto(1, "Shared"));
    //    // businessOwned is filtered — no mapping

    //    // Act
    //    var result = await _productService.GetAllGroupedAsync(exclusive: false);

    //    // Assert
    //    Assert.Multiple(() =>
    //    {
    //        Assert.That(result.Success, Is.True);
    //        Assert.That(result.Data!, Has.Count.EqualTo(1));
    //        Assert.That(result.Data.First().Products, Has.Count.EqualTo(1));
    //        Assert.That(result.Data.First().Products.First().Name, Is.EqualTo("Shared"));
    //    });
    //}

    //[Test]
    //public async Task GetFilteredAsync_Default_ReturnsVisibleProducts()
    //{
    //    // Arrange
    //    var staff = TestDataFactory.CreateValidStaff(role: EmployeeRole.regular, businessid: 1);
    //    _currentUser.GetCurrentUserAsync().Returns(staff);

    //    var p1 = TestDataFactory.CreateValidProduct(1, businessId: 1, name: "Espresso");
    //    var p2 = TestDataFactory.CreateSharedProduct(2, name: "Macchiato");

    //    var queryable = new List<Product> { p1, p2 }.AsQueryable().BuildMock(); // using MockQueryable

    //    _repository.QueryIncluding(Arg.Any<Expression<Func<Product, object>>>()).Returns(queryable);

    //    _mapper.Map<List<ProductBaseDto>>(Arg.Any<List<Product>>()).Returns(
    //        new List<ProductBaseDto>
    //        {
    //        TestDataFactory.CreateProductBaseDto(1, "Espresso"),
    //        TestDataFactory.CreateProductBaseDto(2, "Macchiato")
    //        });

    //    // Act
    //    var result = await _productService.GetFilteredAsync();

    //    // Assert
    //    Assert.That(result.Success, Is.True);
    //    Assert.That(result.Data, Has.Count.EqualTo(2));
    //}

}