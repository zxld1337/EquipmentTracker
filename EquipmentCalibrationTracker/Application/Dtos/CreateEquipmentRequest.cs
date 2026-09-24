using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class CreateEquipmentRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(1, 120)]
    public int CalibrationIntervalMonths { get; set; }
}
