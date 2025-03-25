using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.Exceptions;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace BartenderTests;
[TestFixture]
public class ProductsServiceTests
{
    private IRepository<Products> _repository;
    private IRepository<ProductCategory> _categoryRepository;
    private IMapper _mapper;
    private ProductsService _productsService;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IRepository<Products>>();
        _categoryRepository = Substitute.For<IRepository<ProductCategory>>();
        _mapper = Substitute.For<IMapper>();
        _productsService = new ProductsService(_repository, _categoryRepository, _mapper);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = new Products
        {
            Id = 1,
            Name = "Espresso",
            Volume = "ŠAL",
            CategoryId = 2,
            Category = new ProductCategory { Id = 2, Name = "Coffee" }
        };

        var productDto = new ProductsDTO
        {
            Id = 1,
            Name = "Espresso",
            Volume = "ŠAL",
            Category = new ProductCategoryDTO { Id = 2, Name = "Coffee" }
        };

        _repository.GetByIdAsync(1).Returns(Task.FromResult((Products?)product));
        _mapper.Map<ProductsDTO>(product).Returns(productDto);

        // Act
        var result = await _productsService.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Espresso"));
            Assert.That(result.Volume, Is.EqualTo("ŠAL"));
            Assert.That(result.Category.Id, Is.EqualTo(2));
            Assert.That(result.Category.Name, Is.EqualTo("Coffee"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotExists()
    {
        // Arrange
        _repository.GetByIdAsync(1).Returns(Task.FromResult((Products?)null));

        // Act
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.GetByIdAsync(1));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Product with id 1 not found"));
    }

    [Test]
    public async Task GetAllAsync_NotGrouped_ReturnsProductDTOList()
    {
        // Arrange
        var products = new List<Products>()
        {
            new() {Id = 1, Name = "Product 1", Volume = "ŠAL", CategoryId = 2 },
            new() {Id = 2, Name = "Product 2", Volume = "ŠAL", CategoryId = 2 }
        };

        var productDtos = products.Select(p => new ProductsDTO
        {
            Id = p.Id,
            Name = p.Name,
            Volume = p.Volume,
            Category = new ProductCategoryDTO { Id = p.CategoryId, Name = "Category" }
        }).ToList();

        _repository.GetAllWithDetailsAsync().Returns(Task.FromResult(products));
        _mapper.Map<IEnumerable<ProductsDTO>>(products).Returns(productDtos);

        //Act
        var result = await _productsService.GetAllAsync(false);

        // Assert
        Assert.That(result, Is.InstanceOf<IEnumerable<ProductsDTO>>());
        var resultList = result as IEnumerable<ProductsDTO>;
        Assert.That(resultList.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetAllAsync_NotGrouped_ThrowsNotFoundException_WhenNoProducts()
    {
        // Arrange
        _repository.GetAllWithDetailsAsync().Returns(new List<Products>());

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(() => _productsService.GetAllAsync(false));
    }

    [Test]
    public async Task GetAllAsync_Grouped_ReturnsGroupedProductsDTOList()
    {
        // Arrange
        var categories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    Id = 1,
                    Name = "Category 1",
                    Products = new List<Products>
                    {
                        new Products { Id = 1, Name = "Product 1", Volume = "1L" },
                        new Products { Id = 2, Name = "Product 2", Volume = "2L" }
                    }
                }
            };

        var groupedDtos = categories.Select(c => new GroupedProductsDTO
        {
            Category = c.Name,
            Products = c.Products.Select(p => new ProductsBaseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Volume = p.Volume
            })
        }).ToList();

        _categoryRepository.GetAllWithDetailsAsync().Returns(categories);
        _mapper.Map<IEnumerable<GroupedProductsDTO>>(categories).Returns(groupedDtos);

        // Act
        var result = await _productsService.GetAllAsync(true);

        // Assert
        Assert.That(result, Is.InstanceOf<IEnumerable<GroupedProductsDTO>>());
        var resultList = result as IEnumerable<GroupedProductsDTO>;
        Assert.That(resultList.Count, Is.EqualTo(1));
        Assert.That(resultList.First().Products.Count(), Is.EqualTo(2));
    }

    /*[Test]
    public async Task GetFilteredAsync_FiltersByName_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Products>
        {
            new Products { Id = 1, Name = "product test", Category = new ProductCategory { Name = "Category 1" } },
            new Products { Id = 2, Name = "another product", Category = new ProductCategory { Name = "Category 2" } }
        }.AsQueryable();

        _repository.Query().Returns(products);

        var filteredProducts = products.Where(p => p.Name.ToLower().Contains("test")).ToList();
        var productDtos = filteredProducts
            .Select(p => new ProductsBaseDTO { Id = p.Id, Name = p.Name })
            .ToList();

        _mapper.Map<IEnumerable<ProductsBaseDTO>>(filteredProducts).Returns(productDtos);

        // Act
        var result = await _productsService.GetFilteredAsync(name: "test");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Does.Contain("product test"));
    }


    [Test]
    public async Task GetFilteredAsync_FiltersByCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Products>
        {
            new Products { Id = 1, Name = "Product 1", Category = new ProductCategory {Id = 1, Name = "Drinks" } },
            new Products { Id = 2, Name = "Product 2", Category = new ProductCategory {Id = 2, Name = "Food" } }
        };

        _repository.Query().Returns(products.AsQueryable());

        var filteredProducts = products.Where(p => p.Category.Name.ToLower().Contains("drink")).ToList();
        var productDtos = filteredProducts
            .Select(p => new ProductsBaseDTO { Id = p.Id, Name = p.Name })
            .ToList();

        _mapper.Map<IEnumerable<ProductsBaseDTO>>(filteredProducts).Returns(productDtos);

        // Act
        var result = await _productsService.GetFilteredAsync(category: "drink");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Product 1"));
    }

    [Test]
    public async Task GetFilteredAsync_FiltersByNameAndCategory_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Products>
        {
            new Products { Id = 1, Name = "Test Drink", Category = new ProductCategory { Name = "Drinks" } },
            new Products { Id = 2, Name = "Test Food", Category = new ProductCategory { Name = "Food" } },
            new Products { Id = 3, Name = "Another Drink", Category = new ProductCategory { Name = "Drinks" } }
        };

        var query = products.AsQueryable();
        _repository.Query().Returns(query);

        var filteredProducts = products.Where(p => p.Name.ToLower().Contains("test") && p.Category.Name.ToLower().Contains("drink")).ToList();
        var productDtos = filteredProducts
            .Select(p => new ProductsBaseDTO { Id = p.Id, Name = p.Name })
            .ToList();

        _mapper.Map<IEnumerable<ProductsBaseDTO>>(filteredProducts).Returns(productDtos);

        // Act
        var result = await _productsService.GetFilteredAsync(name: "test", category: "drink");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("Test Drink"));
    }

    [Test]
    public void GetFilteredAsync_NoMatches_ThrowsNotFoundException()
    {
        // Arrange
        var products = new List<Products>
        {
            new Products { Id = 1, Name = "Product 1", Category = new ProductCategory { Name = "Category 1" } }
        };

        var query = products.AsQueryable();
        _repository.Query().Returns(query);

        // Act
        var ex = Assert.ThrowsAsync<NotFoundException>(() =>
            _productsService.GetFilteredAsync(name: "nonexistent"));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("No products found matching the criteria"));
    }

    [Test]
    public async Task GetFilteredAsync_NoFilters_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Products>
        {
            new Products { Id = 1, Name = "Product 1" },
            new Products { Id = 2, Name = "Product 2" }
        };

        var query = products.AsQueryable();
        _repository.Query().Returns(query);

        var productDtos = products
            .Select(p => new ProductsBaseDTO { Id = p.Id, Name = p.Name })
            .ToList();

        _mapper.Map<IEnumerable<ProductsBaseDTO>>(products).Returns(productDtos);

        // Act
        var result = await _productsService.GetFilteredAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetSortedByNameAsync_ReturnsProductsInCorrectOrder()
    {
        // Arrange
        var unsortedProducts = new List<Products>
        {
            new Products { Id = 2, Name = "Latte", Category = new ProductCategory { Id = 1, Name = "Drink" } },
            new Products { Id = 1, Name = "Coffee", Category = new ProductCategory { Id = 1, Name = "Drink" } },
            new Products { Id = 3, Name = "Espresso", Category = new ProductCategory { Id = 1, Name = "Drink" } }
        };

        var sortedProducts = unsortedProducts.OrderBy(p => p.Name).ToList();
        var productDtos = sortedProducts.Select(p => new ProductsDTO
        {
            Id = p.Id,
            Name = p.Name,
            Category = new ProductCategoryDTO { Id = p.Category.Id, Name = p.Category.Name }
        }).ToList();

        _repository.Query().Returns(unsortedProducts.AsQueryable());
        _mapper.Map<IEnumerable<ProductsDTO>>(sortedProducts).Returns(productDtos);

        // Act
        var result = (await _productsService.GetSortedByNameAsync()).ToList();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Coffee"));
        Assert.That(result[1].Name, Is.EqualTo("Espresso"));
        Assert.That(result[2].Name, Is.EqualTo("Latte"));
    }

    [Test]
    public void GetSortedByNameAsync_ThrowsNotFoundException_WhenNoProducts()
    {
        // Arrange
        _repository.Query().Returns(new List<Products>().AsQueryable());

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.GetSortedByNameAsync());
        Assert.That(ex.Message, Is.EqualTo("There are currently no products"));
    }*/

    [Test]
    public async Task AddAsync_ValidProduct_AddsProduct()
    {
        // Arrange
        var productDto = new UpsertProductDTO
        {
            Name = "New Product",
            Volume = "1L",
            CategoryId = 1
        };
        var product = new Products { Name = "New Product", Volume = "1L", CategoryId = 1 };

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
            .Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
            .Returns(false);
        _mapper.Map<Products>(productDto).Returns(product);

        // Act
        await _productsService.AddAsync(productDto);

        // Assert
        await _repository.Received(1).AddAsync(product);
    }

    [Test]
    public void AddAsync_ThrowsDuplicateEntry_WhenProductExists()
    {
        // Arrange
        var productDto = new UpsertProductDTO
        {
            Name = "Existing Product",
            Volume = "1L",
            CategoryId = 1
        };

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
            .Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
            .Returns(true);

        // Act & Assert
        Assert.ThrowsAsync<DuplicateEntryException>(() => _productsService.AddAsync(productDto));
    }

    [Test]
    public void AddAsync_ThrowsValidationException_WhenNameEmpty()
    {
        // Arrange
        var productDto = new UpsertProductDTO
        {
            Name = "",
            Volume = "1L",
            CategoryId = 1
        };

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _productsService.AddAsync(productDto));
    }

    [Test]
    public void AddAsync_ThrowsValidationException_WhenCategoryNotExists()
    {
        // Arrange
        var productDto = new UpsertProductDTO
        {
            Name = "New Product",
            Volume = "1L",
            CategoryId = 999
        };

        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
            .Returns(false);

        // Act & Assert
        Assert.ThrowsAsync<ValidationException>(() => _productsService.AddAsync(productDto));
    }

    [Test]
    public async Task UpdateAsync_ValidUpdate_UpdatesProduct()
    {
        // Arrange
        var productId = 1;
        var productDto = new UpsertProductDTO
        {
            Name = "Updated Product",
            Volume = "2L",
            CategoryId = 1
        };
        var existingProduct = new Products { Id = productId, Name = "Original Product" };

        _repository.GetByIdAsync(productId).Returns(existingProduct);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
            .Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
            .Returns(false);

        // Act
        await _productsService.UpdateAsync(productId, productDto);

        // Assert
        _mapper.Received(1).Map(productDto, existingProduct);
        await _repository.Received(1).UpdateAsync(existingProduct);
    }

    [Test]
    public void UpdateAsync_ThrowsNotFound_WhenProductNotExists()
    {
        // Arrange
        var productId = 999;
        var productDto = new UpsertProductDTO
        {
            Name = "Updated Product",
            Volume = "2L",
            CategoryId = 1
        };

        _repository.GetByIdAsync(productId).Returns((Products)null);

        // Act
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.DeleteAsync(productId));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));
    }

    [Test]
    public void UpdateAsync_ThrowsDuplicate_WhenNameExistsForOtherProduct()
    {
        // Arrange
        var productId = 1;
        var productDto = new UpsertProductDTO
        {
            Name = "Duplicate Product",
            Volume = "1L",
            CategoryId = 1
        };
        var existingProduct = new Products { Id = productId, Name = "Original Product" };

        _repository.GetByIdAsync(productId).Returns(existingProduct);
        _categoryRepository.ExistsAsync(Arg.Any<Expression<Func<ProductCategory, bool>>>())
            .Returns(true);
        _repository.ExistsAsync(Arg.Any<Expression<Func<Products, bool>>>())
            .Returns(true);

        // Act & Assert
        Assert.ThrowsAsync<DuplicateEntryException>(() =>
            _productsService.UpdateAsync(productId, productDto));
    }

    [Test]
    public async Task DeleteAsync_ValidId_DeletesProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Products { Id = productId, Name = "Product to delete" };

        _repository.GetByIdAsync(productId).Returns(product);

        // Act
        await _productsService.DeleteAsync(productId);

        // Assert
        await _repository.Received(1).DeleteAsync(product);
    }

    [Test]
    public void DeleteAsync_ThrowsNotFound_WhenProductNotExists()
    {
        // Arrange
        var productId = 999;
        _repository.GetByIdAsync(productId).Returns((Products)null);

        // Act
        var ex = Assert.ThrowsAsync<NotFoundException>(() => _productsService.DeleteAsync(productId));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Product with id 999 not found"));
    }
}

