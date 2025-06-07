using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
using BartenderTests.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace BartenderTests.IntegrationTests.Services;

[TestFixture]
public class TableManagementServiceIntegrationTests : IntegrationTestBase
{
    private ITableManagementService _service = null!;
    private ITableRepository _tableRepo = null!;
    private MockCurrentUser _mockUser = null!;

    [SetUp]
    public void SetUp()
    {
        var scope = Factory.Services.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<ITableManagementService>();
        _tableRepo = scope.ServiceProvider.GetRequiredService<ITableRepository>();
        _mockUser = scope.ServiceProvider.GetRequiredService<MockCurrentUser>();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnTablesForCurrentUser()
    {
        // Arrange
        var table1 = new Table { Label = "T11", PlaceId = 1, Width = 100, Height = 100, X = 10, Y = 10 };
        var table2 = new Table { Label = "T22", PlaceId = 1, Width = 120, Height = 100, X = 20, Y = 20 };
        var otherPlaceTable = new Table { Label = "X1", PlaceId = 2, Width = 100, Height = 100, X = 10, Y = 10 };

        await _tableRepo.AddMultipleAsync([table1, table2, otherPlaceTable]);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(result.Select(r => r.Label), Does.Contain("T11").And.Contain("T22").And.Not.Contain("X1"));
    }

    [Test]
    public async Task GetByPlaceId_ShouldReturnOnlyActiveTables()
    {
        // Arrange
        var table1 = new Table {Label = "Active1", PlaceId = 3, Width = 100, Height = 100, X = 10, Y = 10 };
        var table2 = new Table {Label = "Active2", PlaceId = 3, Width = 100, Height = 100, X = 10, Y = 10 };
        var deletedTable = new Table { Label = "Deleted", PlaceId = 3, DeletedAt = DateTime.UtcNow, Width = 100, Height = 100, X = 10, Y = 10 };
        var otherPlaceTable = new Table { Label = "WrongPlace", PlaceId = 4, Width = 100, Height = 100, X = 10, Y = 10 };

        await _tableRepo.AddMultipleAsync([table1, table2, deletedTable, otherPlaceTable]);

        // Act
        var result = await _service.GetByPlaceId(3);

        // Assert
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(result.Select(t => t.Label), Does.Contain("Active1").And.Contain("Active2").And.Not.Contain("Deleted"));
    }

    [Test]
    public async Task GetByLabelAsync_ShouldReturnTableForCurrentUser()
    {
        // Arrange
        var table = new Table { Label = "SINGLE", PlaceId = 4, Width = 100, Height = 100, X = 10, Y = 10 };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 4, businessid: 1));

        // Act
        var result = await _service.GetByLabelAsync("SINGLE");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Label, Is.EqualTo("SINGLE"));
    }

    [Test]
    public void GetByLabelAsync_ShouldThrow_WhenTableNotFound()
    {
        // Arrange
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 5, businessid: 1));

        // Act & Assert
        var ex = Assert.ThrowsAsync<TableNotFoundException>(() =>
            _service.GetByLabelAsync("MISSING"));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task BulkUpsertAsync_ShouldInsertNewTables()
    {
        // Arrange
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 6, businessid: 1));

        var dtoList = new List<UpsertTableDto>
    {
        new() { Label = "TBL-1", Width = 100, Height = 100, X = 10, Y = 10 },
        new() { Label = "TBL-2", Width = 150, Height = 150, X = 20, Y = 20 }
    };

        // Act
        await _service.BulkUpsertAsync(dtoList);

        // Assert
        var all = await _tableRepo.GetAllByPlaceAsync(6);
        Assert.That(all, Has.Count.EqualTo(2));
        Assert.That(all.Select(t => t.Label), Contains.Item("TBL-1").And.Contains("TBL-2"));
    }


    [Test]
    public async Task BulkUpsertAsync_ShouldUpdateExistingTables()
    {
        // Arrange
        var existing = new Table { Label = "EXIST", Width = 50, Height = 50, X = 0, Y = 0, PlaceId = 7 };
        await _tableRepo.AddAsync(existing);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 7, businessid: 1));

        var dtoList = new List<UpsertTableDto>
    {
        new() { Label = "EXIST", Width = 200, Height = 200, X = 25, Y = 25 } // changed dimensions
    };

        // Act
        await _service.BulkUpsertAsync(dtoList);

        // Assert
        var updated = await _tableRepo.GetByPlaceLabelAsync(7, "EXIST");
        Assert.That(updated!.Width, Is.EqualTo(200));
        Assert.That(updated.Height, Is.EqualTo(200));
        Assert.That(updated.X, Is.EqualTo(25));
        Assert.That(updated.Y, Is.EqualTo(25));
    }

    [Test]
    public void BulkUpsertAsync_ShouldThrow_WhenDuplicateLabelsInInput()
    {
        // Arrange
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 8, businessid: 1));

        var dtoList = new List<UpsertTableDto>
    {
        new() { Label = "DUPL" },
        new() { Label = "dupl" } // case-insensitive
    };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(() =>
            _service.BulkUpsertAsync(dtoList));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("Duplicate labels"));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveTable_WhenLabelExists()
    {
        var table = new Table { Label = "DEL1", PlaceId = 10, Width = 100, Height = 100, X = 10, Y = 10 };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 10, businessid: 1));

        await _service.DeleteAsync("DEL1");

        var deleted = await _tableRepo.GetByPlaceLabelAsync(10, "DEL1");
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public void DeleteAsync_ShouldThrow_WhenTableNotFound()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 11, businessid: 1));

        var ex = Assert.ThrowsAsync<TableNotFoundException>(() =>
            _service.DeleteAsync("NOT-FOUND"));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task RegenerateSaltAsync_ShouldUpdateSalt_ForTable()
    {
        var table = new Table { Label = "RESALT", PlaceId = 12, QrSalt = "original", Width = 100, Height = 100, X = 10, Y = 10 };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 12, businessid: 1));

        var newSalt = await _service.RegenerateSaltAsync("RESALT");

        Assert.That(newSalt, Is.Not.Null.And.Not.EqualTo("original"));

        var updated = await _tableRepo.GetByPlaceLabelAsync(12, "RESALT");
        Assert.That(updated!.QrSalt, Is.EqualTo(newSalt));
    }

    [Test]
    public async Task SwitchDisabledAsync_ShouldSetDisabledFlag()
    {
        var table = new Table { Label = "DISABLE1", PlaceId = 1, IsDisabled = false, Width = 100, Height = 100, X = 10, Y = 10 };
        await _tableRepo.AddAsync(table);

        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        await _service.SwitchDisabledAsync("DISABLE1", true);

        var updated = await _tableRepo.GetByPlaceLabelAsync(1, "DISABLE1");
        Assert.That(updated!.IsDisabled, Is.True);
    }

    [Test]
    public void SwitchDisabledAsync_ShouldThrow_WhenTableMissing()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 14, businessid: 1));

        var ex = Assert.ThrowsAsync<TableNotFoundException>(() =>
            _service.SwitchDisabledAsync("NONEXIST", true));

        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task BulkUpsertAsync_ShouldBeIdempotent_WhenSameInputIsUsedTwice()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));
        var dtoList = new List<UpsertTableDto>
    {
        new() { Label = "TABLE1", Width = 100, Height = 100, X = 10, Y = 10 }
    };

        await _service.BulkUpsertAsync(dtoList);
        await _service.BulkUpsertAsync(dtoList); // Should update, not insert again

        var all = await _tableRepo.GetAllByPlaceAsync(1);
        Assert.That(all, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(all.Last().Label, Is.EqualTo("TABLE1"));
    }

    [Test]
    public void RegenerateSaltAsync_ShouldThrow_WhenTableMissing()
    {
        _mockUser.Override(TestDataFactory.CreateValidStaff(placeid: 1, businessid: 1));

        var ex = Assert.ThrowsAsync<TableNotFoundException>(() =>
            _service.RegenerateSaltAsync("MISSING"));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Does.Contain("not found"));
    }

}
