using Application.Common;
using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using UnitTests.Application.Fakes;

namespace UnitTests.Application.Services;
public class EquipmentServiceTests
{
    private readonly FakeEquipmentRepository _repository = new();
    private readonly EquipmentService _service;

    public EquipmentServiceTests()
    {
        _service = new EquipmentService(_repository);
    }


    [Fact]
    public async Task CreateAsync_WithValidRequest_SavesEquipment()
    {
        var request = new CreateEquipmentRequest
        {
            Name = "Multimeter",
            Barcode = "EQ-001",
            Location = "Lab 1",
            CalibrationIntervalMonths = 12,
        };

        var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);


        Assert.True(result.IsSuccess);

        var saved = Assert.Single(_repository.Equipments);
        Assert.Equal("EQ-001", saved.Barcode);
        Assert.Equal(1, _repository.SaveChangesCallCount);
    }


    [Fact]
    public async Task CreateAsync_WhenBarcodeAlreadyExists_ReturnsConflict()
    {
        _repository.Equipments.Add(new Equipment { Id = 1, Barcode = "EQ-001" });
        var request = new CreateEquipmentRequest
        {
            Name = "Other",
            Barcode = "EQ-001",
            Location = "Lab 2",
            CalibrationIntervalMonths = 6,
        };

        var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Single(_repository.Equipments);
        Assert.Equal(0, _repository.SaveChangesCallCount);
    }


    [Fact]
    public async Task CreateAsync_WhenBarcodeExistWithSurroundingWhiteSpace_ReturnsConflict()
    {
        _repository.Equipments.Add(new Equipment { Id = 1, Barcode = "EQ-001" });
        var request = new CreateEquipmentRequest
        {
            Name = "Other",
            Barcode = " EQ-001 ",
            Location = "Lab 2",
            CalibrationIntervalMonths = 6,
        };

        var result = await _service.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(0, _repository.SaveChangesCallCount);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    [InlineData(-5)]
    public async Task GetDueAsync_WhenDaysOutOfRange_ReturnsValidation(int days)
    {
        var result = await _service.GetDueAsync(days, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(365)]
    public async Task GetDueAsync_WhenDaysAtBoundary_IncludesLastDayAndExcludesNextDay(int days)
    {
        var today = DateTime.Today;
        _repository.Equipments.AddRange(
        [
            new Equipment { Id = 1, NextDueDate = today.AddDays(days) },
            new Equipment { Id = 2, NextDueDate = today.AddDays(days + 1) }
        ]);

        var result = await _service.GetDueAsync(days, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal([1], result.Value!.Select(e => e.Id));
    }


    [Fact]
    public async Task GetDueAsync_WhenActiveItemIsOverdue_IncludesIt()
    {
        _repository.Equipments.Add(new Equipment
        {
            Id = 1,
            Status = EquipmentStatus.Active,
            NextDueDate = DateTime.Today.AddDays(-1)
        });

        var result = await _service.GetDueAsync(30, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value!);
        Assert.Equal(1, item.Id);
        Assert.True(item.IsDue);
    }


    [Fact]
    public async Task GetDueAsync_WithMixedEquipment_ReturnsOnlyActiveDueWithinWindowSortedByNextDueDate()
    {
        var today = DateTime.Today;
        _repository.Equipments.AddRange(
        [
            new Equipment { Id = 1, NextDueDate = today.AddDays(10) },
            new Equipment { Id = 2, NextDueDate = today.AddDays(-3) },
            new Equipment { Id = 3, NextDueDate = today.AddDays(30) },
            new Equipment { Id = 4, NextDueDate = today.AddDays(31) },
            new Equipment { Id = 5, NextDueDate = today.AddDays(5), Status = EquipmentStatus.Retired },
            new Equipment { Id = 6, NextDueDate = null },
            new Equipment { Id = 7, NextDueDate = today.AddDays(1), DeletedAt = DateTime.UtcNow }
        ]);

        var result = await _service.GetDueAsync(30, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal([2, 1, 3], result.Value!.Select(e => e.Id));
    }
}
