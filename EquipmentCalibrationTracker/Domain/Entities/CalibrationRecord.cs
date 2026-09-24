using Domain.Enums;

namespace Domain.Entities;

public class CalibrationRecord
{
    public int Id { get; set; }

    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public DateTime PerformedOn { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public CalibrationResult Result { get; set; }
    public string? Notes { get; set; }
}