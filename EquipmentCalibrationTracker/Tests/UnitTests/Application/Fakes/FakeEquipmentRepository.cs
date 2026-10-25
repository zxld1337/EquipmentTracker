using Application.Abstractions;
using Domain.Entities;

namespace UnitTests.Application.Fakes;
public class FakeEquipmentRepository : IEquipmentRepository
{
    public List<Equipment> Equipments { get; } = new();
    public int SaveChangesCallCount { get; private set; }

    
    public Task AddAsync(Equipment equipment, CancellationToken ct = default)
    {
        Equipments.Add(equipment);
        return Task.CompletedTask;
    }


    public Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null, CancellationToken ct = default)
        => Task.FromResult(Equipments.Any(e =>
                    e.DeletedAt == null
                    && e.Barcode == barcode
                    && (excludeId == null || e.Id != excludeId)));


    public Task<List<Equipment>> GetAllAsync(CancellationToken ct = default) 
        => Task.FromResult(Equipments.Where(e => e.DeletedAt == null).ToList());


    public Task<Equipment?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Equipments.FirstOrDefault(e => e.Id == id && e.DeletedAt == null));


    public Task<CalibrationRecord?> GetCalibrationAsync(int equipmentId, int calibrationId, CancellationToken ct = default)
     => Task.FromResult(Equipments
         .SelectMany(e => e.Calibrations)
         .FirstOrDefault(c => c.EquipmentId == equipmentId && c.Id == calibrationId));


    public Task<Equipment?> GetWithCalibrationsAsync(int id, CancellationToken ct = default)
        => GetByIdAsync(id, ct);


    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
