using Application.Common;
using Application.Dtos;
using Application.Services;
using Domain.Entities;
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
}
