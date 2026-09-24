using Application.Abstractions;
using Application.Common;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class EquipmentService
{
    private readonly IEquipmentRepository _repository;

    public EquipmentService(IEquipmentRepository repository)
    {
        _repository = repository;
    }


    public async Task<List<EquipmentListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var equipment = await _repository.GetAllAsync(ct);
        return equipment.Select(e => e.ToListItemDto(DateTime.Today)).ToList();
    }


    public async Task<Result<EquipmentDetailsDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var equipment = await _repository.GetWithCalibrationsAsync(id, ct);

        if (equipment is null)
            return Result<EquipmentDetailsDto>.Failure(
                ErrorType.NotFound, $"Equipment {id} was not found.");

        return Result<EquipmentDetailsDto>.Success(equipment.ToDetailsDto(DateTime.Today));
    }

    public async Task<Result<EquipmentListItemDto>> CreateAsync(
        CreateEquipmentRequest request, CancellationToken ct = default)
    {
        if (await _repository.BarcodeExistsAsync(request.Barcode, null, ct))
            return Result<EquipmentListItemDto>.Failure(
                ErrorType.Conflict, $"Barcode '{request.Barcode}' is already in use.");

        var equipment = new Equipment
        {
            Name = request.Name.Trim(),
            Barcode = request.Barcode.Trim(),
            Location = request.Location.Trim(),
            CalibrationIntervalMonths = request.CalibrationIntervalMonths,
            Status = EquipmentStatus.Active,
            NextDueDate = null
        };

        await _repository.AddAsync(equipment, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<EquipmentListItemDto>.Success(MapToListItem(equipment));
    }

    public async Task<Result<EquipmentListItemDto>> UpdateAsync(
        int id, UpdateEquipmentRequest request, CancellationToken ct = default)
    {
        var equipment = await _repository.GetByIdAsync(id, ct);

        if (equipment is null)
            return Result<EquipmentListItemDto>.Failure(
                ErrorType.NotFound, $"Equipment {id} was not found.");

        if (await _repository.BarcodeExistsAsync(request.Barcode, id, ct))
            return Result<EquipmentListItemDto>.Failure(
                ErrorType.Conflict, $"Barcode '{request.Barcode}' is already in use.");

        equipment.Name = request.Name.Trim();
        equipment.Barcode = request.Barcode.Trim();
        equipment.Location = request.Location.Trim();
        equipment.CalibrationIntervalMonths = request.CalibrationIntervalMonths;

        await _repository.SaveChangesAsync(ct);

        return Result<EquipmentListItemDto>.Success(MapToListItem(equipment));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        var equipment = await _repository.GetByIdAsync(id, ct);

        if (equipment is null)
            return Result<bool>.Failure(ErrorType.NotFound, $"Equipment {id} was not found.");

        equipment.DeletedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    private static EquipmentListItemDto MapToListItem(Equipment e)
        => new()
        {
            Id = e.Id,
            Name = e.Name,
            Barcode = e.Barcode,
            Location = e.Location,
            CalibrationIntervalMonths = e.CalibrationIntervalMonths,
            NextDueDate = e.NextDueDate,
            IsDue = e.NextDueDate is not null && e.NextDueDate <= DateTime.Today
        };
}
