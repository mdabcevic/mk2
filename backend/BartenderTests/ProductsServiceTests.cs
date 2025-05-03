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





    //Tests for filter missing


    //[Test]
    //public async Task AddAsync_ValidProduct_AddsProduct()
    //{
    //    // Arrange
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "New Product",
    //        Volume = "1L",
    //        CategoryId = 1
    //    };
    //    var product = new Products { Name = "New Product", Volume = "1L", CategoryId = 1 };

    //    _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
    //        .Returns(true);
    //    _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
    //        .Returns(false);
    //    _mapper.Map<Products>(productDto).Returns(product);

    //    // Act
    //    await _productsService.AddAsync(productDto);

    //    // Assert
    //    await _repository.Received(1).AddAsync(product);
    //}

    //[Test]
    //public void AddAsync_ThrowsDuplicateEntry_WhenProductExists()
    //{
    //    // Arrange
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "Existing Product",
    //        Volume = "1L",
    //        CategoryId = 1
    //    };

    //    _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
    //        .Returns(true);
    //    _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
    //        .Returns(true);

    //    // Act & Assert
    //    Assert.ThrowsAsync<DuplicateEntryException>(() => _productsService.AddAsync(productDto));
    //}

    //[Test]
    //public void AddAsync_ThrowsValidationException_WhenNameEmpty()
    //{
    //    // Arrange
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "",
    //        Volume = "1L",
    //        CategoryId = 1
    //    };

    //    // Act & Assert
    //    Assert.ThrowsAsync<ValidationException>(() => _productsService.AddAsync(productDto));
    //}

    //[Test]
    //public void AddAsync_ThrowsValidationException_WhenCategoryNotExists()
    //{
    //    // Arrange
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "New Product",
    //        Volume = "1L",
    //        CategoryId = 999
    //    };

    //    _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
    //        .Returns(false);

    //    // Act & Assert
    //    Assert.ThrowsAsync<ValidationException>(() => _productsService.AddAsync(productDto));
    //}

    //[Test]
    //public async Task UpdateAsync_ValidUpdate_UpdatesProduct()
    //{
    //    // Arrange
    //    var productId = 1;
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "Updated Product",
    //        Volume = "2L",
    //        CategoryId = 1
    //    };
    //    var existingProduct = new Products { Id = productId, Name = "Original Product" };

    //    _repository.GetByIdAsync(productId).Returns(existingProduct);
    //    _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
    //        .Returns(true);
    //    _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
    //        .Returns(false);

    //    // Act
    //    await _productsService.UpdateAsync(productId, productDto);

    //    // Assert
    //    _mapper.Received(1).Map(productDto, existingProduct);
    //    await _repository.Received(1).UpdateAsync(existingProduct);
    //}

    //[Test]
    //public void UpdateAsync_ThrowsNotFound_WhenProductNotExists()
    //{
    //    // Arrange
    //    var productId = 999;
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "Updated Product",
    //        Volume = "2L",
    //        CategoryId = 1
    //    };

    //    _repository.GetByIdAsync(productId).Returns((Products)null);

    //    // Act
    //    var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.DeleteAsync(productId));

    //    // Assert
    //    Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));
    //}

    //[Test]
    //public void UpdateAsync_ThrowsDuplicate_WhenNameExistsForOtherProduct()
    //{
    //    // Arrange
    //    var productId = 1;
    //    var productDto = new UpsertProductDTO
    //    {
    //        Name = "Duplicate Product",
    //        Volume = "1L",
    //        CategoryId = 1
    //    };
    //    var existingProduct = new Products { Id = productId, Name = "Original Product" };

    //    _repository.GetByIdAsync(productId).Returns(existingProduct);
    //    _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
    //        .Returns(true);
    //    _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
    //        .Returns(true);

    //    // Act & Assert
    //    Assert.ThrowsAsync<DuplicateEntryException>(() =>
    //        _productsService.UpdateAsync(productId, productDto));
    //}

    //[Test]
    //public async Task DeleteAsync_ValidId_DeletesProduct()
    //{
    //    // Arrange
    //    var productId = 1;
    //    var product = new Products { Id = productId, Name = "Product to delete" };

    //    _repository.GetByIdAsync(productId).Returns(product);

    //    // Act
    //    await _productsService.DeleteAsync(productId);

    //    // Assert
    //    await _repository.Received(1).DeleteAsync(product);
    //}

    //[Test]
    //public void DeleteAsync_ThrowsNotFound_WhenProductNotExists()
    //{
    //    // Arrange
    //    var productId = 999;
    //    _repository.GetByIdAsync(productId).Returns((Products)null);

    //    // Act
    //    var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.DeleteAsync(productId));

    //    // Assert
    //    Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));
    //}
}

