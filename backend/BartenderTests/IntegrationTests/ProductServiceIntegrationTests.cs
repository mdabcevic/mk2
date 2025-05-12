using Bartender.Data.Models;
using Bartender.Domain.DTO.Product;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests;

[TestFixture]
public class ProductServiceIntegrationTests : IntegrationTestBase
{
    private IProductService _service = null!;
    private IRepository<Product> _productRepo = null!;
    private IRepository<ProductCategory> _categoryRepo = null!;
    private IRepository<Business> _businessRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IProductService>();
        _productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        _categoryRepo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCategory>>();
        _businessRepo = scope.ServiceProvider.GetRequiredService<IRepository<Business>>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task AddAsync_ShouldCreateProduct_WhenValid()
    {
        var category = new ProductCategory { Name = "Coffee" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "BrewCo", OIB = "12345678901" };
        await _businessRepo.AddAsync(business);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var dto = new UpsertProductDto
        {
            Name = "Espresso",
            CategoryId = category.Id,
            Volume = "30ml",
        };

        await _service.AddAsync(dto);

        var exists = await _productRepo.ExistsAsync(p => p.Name == "Espresso");
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenProductExists()
    {
        var category = new ProductCategory { Name = "Coffee" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "BrewCo", OIB = "12345678901" };
        await _businessRepo.AddAsync(business);

        var product = new Product
        {
            Name = "Espresso",
            Volume = "30ml",
            CategoryId = category.Id,
            BusinessId = business.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var dto = new UpsertProductDto
        {
            Name = "Espresso",
            Volume = "30ml",
            CategoryId = category.Id
        };

        var ex = Assert.ThrowsAsync<ConflictException>(() => _service.AddAsync(dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task AddAsync_ShouldFail_WhenCategoryInvalid()
    {
        var business = new Business { Name = "InvalidCatBiz", OIB = "11111111111" };
        await _businessRepo.AddAsync(business);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var dto = new UpsertProductDto
        {
            Name = "Americano",
            Volume = "120ml",
            CategoryId = 9999 // nonexistent
        };

        var ex = Assert.ThrowsAsync<NotFoundException>(() => _service.AddAsync(dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenAuthorized()
    {
        var category = new ProductCategory { Name = "Tea" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "TeaCo", OIB = "44444444444" };
        await _businessRepo.AddAsync(business);

        var product = new Product
        {
            Name = "Green Tea",
            Volume = "250ml",
            CategoryId = category.Id,
            BusinessId = business.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var dto = new UpsertProductDto
        {
            Name = "Green Tea",
            Volume = "300ml",
            CategoryId = category.Id,
            BusinessId = business.Id
        };

        await _service.UpdateAsync(product.Id, dto);

        var updated = await _productRepo.GetByIdAsync(product.Id);
        Assert.That(updated!.Volume, Is.EqualTo("300ml"));
    }

    [Test]
    public async Task UpdateAsync_ShouldFail_WhenUnauthorized()
    {
        var category = new ProductCategory { Name = "Soft Drinks" };
        await _categoryRepo.AddAsync(category);

        var business1 = new Business { Name = "Owner A", OIB = "55555555555" };
        var business2 = new Business { Name = "Owner B", OIB = "66666666666" };
        await _businessRepo.AddAsync(business1);
        await _businessRepo.AddAsync(business2);

        var product = new Product
        {
            Name = "Cola",
            Volume = "500ml",
            CategoryId = category.Id,
            BusinessId = business2.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business1.Id));

        var dto = new UpsertProductDto
        {
            Name = "Cola",
            Volume = "500ml",
            CategoryId = category.Id,
            BusinessId = business2.Id
        };

        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.UpdateAsync(product.Id, dto));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldDelete_WhenAuthorized()
    {
        var category = new ProductCategory { Name = "Beer" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "BarCo", OIB = "88888888888" };
        await _businessRepo.AddAsync(business);

        var product = new Product
        {
            Name = "Lager",
            Volume = "500ml",
            CategoryId = category.Id,
            BusinessId = business.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        await _service.DeleteAsync(product.Id);

        var exists = await _productRepo.ExistsAsync(p => p.Id == product.Id);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldFail_WhenUnauthorized()
    {
        var category = new ProductCategory { Name = "Soda" };
        await _categoryRepo.AddAsync(category);

        var business1 = new Business { Name = "One", OIB = "12345000000" };
        var business2 = new Business { Name = "Two", OIB = "99999000000" };
        await _businessRepo.AddAsync(business1);
        await _businessRepo.AddAsync(business2);

        var product = new Product
        {
            Name = "Sprite",
            Volume = "330ml",
            CategoryId = category.Id,
            BusinessId = business2.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business1.Id));

        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.DeleteAsync(product.Id));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void DeleteAsync_ShouldFail_WhenNotFound()
    {
        var ex = Assert.ThrowsAsync<ProductNotFoundException>(() => _service.DeleteAsync(9999));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenAuthorized()
    {
        var category = new ProductCategory { Name = "Wines" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "Winery", OIB = "10101010101" };
        await _businessRepo.AddAsync(business);

        var product = new Product
        {
            Name = "Merlot",
            Volume = "750ml",
            CategoryId = category.Id,
            BusinessId = business.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var result = await _service.GetByIdAsync(product.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Merlot"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldFail_WhenUnauthorized()
    {
        var category = new ProductCategory { Name = "Juices" };
        await _categoryRepo.AddAsync(category);

        var business1 = new Business { Name = "JuiceCo", OIB = "20202020202" };
        var business2 = new Business { Name = "Other", OIB = "30303030303" };
        await _businessRepo.AddAsync(business1);
        await _businessRepo.AddAsync(business2);

        var product = new Product
        {
            Name = "Apple Juice",
            Volume = "250ml",
            CategoryId = category.Id,
            BusinessId = business2.Id
        };
        await _productRepo.AddAsync(product);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business1.Id));

        var ex = Assert.ThrowsAsync<AuthorizationException>(() => _service.GetByIdAsync(product.Id));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnScopedProducts_ForNonAdmin()
    {
        var category = new ProductCategory { Name = "Category1" };
        await _categoryRepo.AddAsync(category);

        var business = new Business { Name = "MyBiz", OIB = "41414141414" };
        await _businessRepo.AddAsync(business);

        var sharedProduct = new Product
        {
            Name = "Shared",
            Volume = null,
            CategoryId = category.Id,
            BusinessId = null
        };

        var ownedProduct = new Product
        {
            Name = "Private",
            Volume = null,
            CategoryId = category.Id,
            BusinessId = business.Id
        };

        await _productRepo.AddAsync(sharedProduct);
        await _productRepo.AddAsync(ownedProduct);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var result = await _service.GetAllAsync();
        var names = result.Select(p => p.Name).ToList();

        Assert.That(names, Does.Contain("Shared"));
        Assert.That(names, Does.Contain("Private"));
    }

    [Test]
    public async Task GetFilteredAsync_ShouldFilterByNameAndCategory()
    {
        var juice = new ProductCategory { Name = "Juices" };
        var soda = new ProductCategory { Name = "Soda" };
        await _categoryRepo.AddAsync(juice);
        await _categoryRepo.AddAsync(soda);

        var business = new Business { Name = "FilterCo", OIB = "51515151515" };
        await _businessRepo.AddAsync(business);

        var product1 = new Product
        {
            Name = "Orange Juice",
            Volume = null,
            CategoryId = juice.Id,
            BusinessId = business.Id
        };

        var product2 = new Product
        {
            Name = "Lemonade",
            Volume = null,
            CategoryId = soda.Id,
            BusinessId = business.Id
        };

        await _productRepo.AddAsync(product1);
        await _productRepo.AddAsync(product2);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var result = await _service.GetFilteredAsync(name: "orange", category: "juice");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Orange Juice"));
    }

    [Test]
    public async Task GetAllGroupedAsync_ShouldReturnGroupedProducts_ForRegularUser()
    {
        var business = new Business { Name = "Grouped Biz", OIB = "71717171717" };
        await _businessRepo.AddAsync(business);

        var juice = new ProductCategory { Name = "Juices" };
        var coffee = new ProductCategory { Name = "Coffee" };
        await _categoryRepo.AddAsync(juice);
        await _categoryRepo.AddAsync(coffee);

        var product1 = new Product
        {
            Name = "Orange Juice",
            CategoryId = juice.Id,
            BusinessId = business.Id
        };

        var product2 = new Product
        {
            Name = "Espresso",
            CategoryId = coffee.Id,
            BusinessId = business.Id
        };

        var sharedProduct = new Product
        {
            Name = "Public Espresso",
            CategoryId = coffee.Id,
            BusinessId = null
        };

        await _productRepo.AddAsync(product1);
        await _productRepo.AddAsync(product2);
        await _productRepo.AddAsync(sharedProduct);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var result = await _service.GetAllGroupedAsync();

        var juiceGroup = result.FirstOrDefault(g => g.Category == "Juices");
        var coffeeGroup = result.FirstOrDefault(g => g.Category == "Coffee");

        Assert.That(juiceGroup, Is.Not.Null);
        Assert.That(coffeeGroup, Is.Not.Null);
        Assert.That(juiceGroup!.Products.Any(p => p.Name == "Orange Juice"), Is.True);
        Assert.That(coffeeGroup!.Products.Any(p => p.Name == "Espresso"), Is.True);
        Assert.That(coffeeGroup.Products.Any(p => p.Name == "Public Espresso"), Is.True);
    }

    [Test]
    public async Task GetAllGroupedAsync_ShouldApplyExclusiveFilter()
    {
        var business = new Business { Name = "Exclusive Test", OIB = "81818181818" };
        await _businessRepo.AddAsync(business);

        var tea = new ProductCategory { Name = "Tea" };
        await _categoryRepo.AddAsync(tea);

        var scopedProduct = new Product
        {
            Name = "Green Tea",
            CategoryId = tea.Id,
            BusinessId = business.Id
        };

        var publicProduct = new Product
        {
            Name = "Public Tea",
            CategoryId = tea.Id,
            BusinessId = null
        };

        await _productRepo.AddAsync(scopedProduct);
        await _productRepo.AddAsync(publicProduct);

        _mockUser.Override(TestDataFactory.CreateValidStaff(businessid: business.Id));

        var exclusiveOnly = await _service.GetAllGroupedAsync(exclusive: true);
        var publicOnly = await _service.GetAllGroupedAsync(exclusive: false);

        Assert.That(exclusiveOnly.SelectMany(g => g.Products).Any(p => p.Name == "Green Tea"), Is.True);
        Assert.That(exclusiveOnly.SelectMany(g => g.Products).Any(p => p.Name == "Public Tea"), Is.False);

        Assert.That(publicOnly.SelectMany(g => g.Products).Any(p => p.Name == "Public Tea"), Is.True);
        Assert.That(publicOnly.SelectMany(g => g.Products).Any(p => p.Name == "Green Tea"), Is.False);
    }

    [Test]
    public async Task GetProductCategoriesAsync_ShouldReturnAll()
    {
        var cat1 = new ProductCategory { Name = "Smoothies" };
        var cat2 = new ProductCategory { Name = "Alcohol" };
        await _categoryRepo.AddAsync(cat1);
        await _categoryRepo.AddAsync(cat2);

        var result = await _service.GetProductCategoriesAsync();

        var names = result.Select(c => c.Name).ToList();
        Assert.That(names, Does.Contain("Smoothies"));
        Assert.That(names, Does.Contain("Alcohol"));
    }

}
