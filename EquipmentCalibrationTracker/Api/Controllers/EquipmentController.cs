using Application.Common;
using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly CalibrationService _calibrationService;
    private readonly EquipmentService _equipmentService;

    public EquipmentController(CalibrationService calibrationService, EquipmentService equipmentService)
    {
        _calibrationService = calibrationService;
        _equipmentService = equipmentService; 
    }

    private ActionResult ToErrorResponse(ErrorType errorType, string? message)
    {
        var statusCode = errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(detail: message, statusCode: statusCode);
    }


    [HttpGet]
    public async Task<ActionResult<List<EquipmentDetailsDto>>> GetAll(
        CancellationToken ct)
    {
        var result = await _equipmentService.GetAllAsync(ct);            
        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<EquipmentDetailsDto>> GetById(
        int id, 
        CancellationToken ct)
    {
        var result = await _equipmentService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return ToErrorResponse(result.ErrorType, result.ErrorMessage);

        return Ok(result.Value);
    }


    [HttpPost]
    public async Task<ActionResult<EquipmentListItemDto>> Create(
        CreateEquipmentRequest request, 
        CancellationToken ct)
    {
        var result = await _equipmentService.CreateAsync(request, ct);

        if (!result.IsSuccess)
            return ToErrorResponse(result.ErrorType, result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EquipmentListItemDto>> Update(
        int id, 
        UpdateEquipmentRequest request, 
        CancellationToken ct)
    {
        var result = await _equipmentService.UpdateAsync(id, request, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResponse(result.ErrorType, result.ErrorMessage);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _equipmentService.DeleteAsync(id, ct);

        return result.IsSuccess
            ? NoContent()
            : ToErrorResponse(result.ErrorType, result.ErrorMessage);
    }


    [HttpPost("{id:int}/calibrations")]
    public async Task<ActionResult<CalibrationRecordDto>> AddCalibration(
        int id,
        CreateCalibrationRequest request,
        CancellationToken ct)
    {
        var result = await _calibrationService.AddCalibrationAsync(id, request, ct);

        if (!result.IsSuccess)
            return ToErrorResponse(result.ErrorType, result.ErrorMessage);

        return CreatedAtAction(
            nameof(GetCalibration),
            new {id, calibrationId = result.Value!.Id},
            result.Value!);
    }

    [HttpGet("{id:int}/calibrations/{calibrationId:int}", Name = nameof(GetCalibration))]
    public async Task<ActionResult<CalibrationRecordDto>> GetCalibration(
        int id, 
        int calibrationId, 
        CancellationToken ct)
    {
        var result = await _calibrationService.GetCalibrationAsync(id, calibrationId, ct);

        if (!result.IsSuccess)
            return ToErrorResponse(result.ErrorType, result.ErrorMessage);

        return Ok(result.Value);
    }

    


    
}
