using Domain.Entities;

namespace Application.Abstractions;


public interface IEquipmentRepository
{
    Task<List<Equipment>> GetAllAsync(CancellationToken ct = default);
    Task<Equipment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Equipment?> GetWithCalibrationsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Equipment equipment, CancellationToken ct = default);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null, CancellationToken ct = default);




    Task<CalibrationRecord?> GetCalibrationAsync(int equipmentId, int calibrationId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
