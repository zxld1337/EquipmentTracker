namespace Application.Dtos;

public class EquipmentDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int CalibrationIntervalMonths { get; set; }
    public DateTime? NextDueDate { get; set; }
    public bool IsDue { get; set; }

    public string Status { get; set; } = string.Empty;
    public List<CalibrationRecordDto> Calibrations { get; set; } = new();
}
