using Application.Abstractions;
using Application.Common;
using Application.Dtos;
using Application.Mappings;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class CalibrationService
{
    private readonly IEquipmentRepository _repository;

    public CalibrationService(IEquipmentRepository repository)
    {
        _repository = repository;
    }


    public async Task<Result<CalibrationRecordDto>> AddCalibrationAsync(
        int equipmentId,
        CreateCalibrationRequest request,
        CancellationToken ct = default)
    {
        var equipment = await _repository.GetWithCalibrationsAsync(equipmentId, ct);

        if (equipment is null)
            return Result<CalibrationRecordDto>.Failure(
                ErrorType.NotFound, $"Equipment {equipmentId} was not found.");

        if (request.PerformedOn.Date > DateTime.Today)
            return Result<CalibrationRecordDto>.Failure(
                ErrorType.Validation, "PerformedOn cannot be in the future.");

        if (equipment.Status == EquipmentStatus.Retired)
            return Result<CalibrationRecordDto>.Failure(
                ErrorType.Conflict, "Cannot add calibration to retired equipment.");

        var record = new CalibrationRecord
        {
            EquipmentId = equipment.Id,
            PerformedOn = request.PerformedOn.Date,
            PerformedBy = request.PerformedBy,
            Result = request.Result,
            Notes = request.Notes
        };

        equipment.Calibrations.Add(record);
        RecalculateNextDueDate(equipment);

        await _repository.SaveChangesAsync(ct);

        return Result<CalibrationRecordDto>.Success(record.ToDto());
    }


    public async Task<Result<CalibrationRecordDto>> GetCalibrationAsync(
        int equipmentId,
        int calibrationId,
        CancellationToken ct = default)
    {
        var record = await _repository.GetCalibrationAsync(equipmentId, calibrationId, ct);

        if (record is null)
            return Result<CalibrationRecordDto>.Failure(
                ErrorType.NotFound,
                $"Calibration {calibrationId} was not found for equipment {equipmentId}.");

        return Result<CalibrationRecordDto>.Success(record.ToDto());
    }


    private static void RecalculateNextDueDate(Equipment equipment)
    {
        var latest = equipment.Calibrations
            .OrderByDescending(c => c.PerformedOn)
            .FirstOrDefault();

        if (latest is null)
        {
            equipment.NextDueDate = null;
            return;
        }

        equipment.NextDueDate = latest.Result == CalibrationResult.Fail
            ? DateTime.Today
            : latest.PerformedOn.AddMonths(equipment.CalibrationIntervalMonths);
    }
}