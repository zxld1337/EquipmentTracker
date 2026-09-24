namespace Application.Dtos;
public class EquipmentListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CalibrationIntervalMonths { get; set; }
    public DateTime? NextDueDate { get; set; }
    public bool IsDue { get; set; }
}
