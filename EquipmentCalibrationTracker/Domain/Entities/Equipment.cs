using Domain.Enums;

namespace Domain.Entities;

public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int CalibrationIntervalMonths { get; set; }
    public DateTime? NextDueDate { get; set; }

    public DateTime? DeletedAt { get; set; }

    public EquipmentStatus Status { get; set; } = EquipmentStatus.Active;

    public List<CalibrationRecord> Calibrations { get; set; } = new();


    public bool IsDue(DateTime asOf)
        => Status == EquipmentStatus.Active
           && NextDueDate.HasValue
           && NextDueDate.Value.Date <= asOf.Date;
}
