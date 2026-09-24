using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class CreateCalibrationRequest
{
    [Required]
    public DateTime PerformedOn { get; set; }

    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(CalibrationResult))]
    public CalibrationResult Result { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
