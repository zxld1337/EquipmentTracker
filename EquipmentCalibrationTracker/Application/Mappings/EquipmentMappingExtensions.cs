using Application.Dtos;
using Domain.Entities;

namespace Application.Mappings;

public static class EquipmentMappingExtensions
{
    public static EquipmentListItemDto ToListItemDto(this Equipment equipment, DateTime asOf)
        => new()
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Barcode = equipment.Barcode,
            Location = equipment.Location,
            Status = equipment.Status.ToString(),
            CalibrationIntervalMonths = equipment.CalibrationIntervalMonths,
            NextDueDate = equipment.NextDueDate,
            IsDue = equipment.IsDue(asOf)
        };

    public static EquipmentDetailsDto ToDetailsDto(this Equipment equipment, DateTime asOf)
        => new()
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Barcode = equipment.Barcode,
            Location = equipment.Location,
            Status = equipment.Status.ToString(),
            CalibrationIntervalMonths = equipment.CalibrationIntervalMonths,
            NextDueDate = equipment.NextDueDate,
            IsDue = equipment.IsDue(asOf),
            Calibrations = equipment.Calibrations
                .OrderByDescending(c => c.PerformedOn)
                .Select(c => c.ToDto())
                .ToList()
        };

    public static CalibrationRecordDto ToDto(this CalibrationRecord record)
        => new()
        {
            Id = record.Id,
            PerformedOn = record.PerformedOn,
            PerformedBy = record.PerformedBy,
            Result = record.Result.ToString(),
            Notes = record.Notes
        };
}