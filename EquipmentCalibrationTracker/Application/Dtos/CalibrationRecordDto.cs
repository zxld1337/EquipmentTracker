namespace Application.Dtos;

public class CalibrationRecordDto
{
    public int Id { get; set; }
    public DateTime PerformedOn { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
