using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext _db;

    public EquipmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Equipments.FirstOrDefaultAsync(e => e.Id == id, ct);


    public Task<Equipment?> GetWithCalibrationsAsync(int id, CancellationToken ct = default)
        => _db.Equipments
            .Include(e => e.Calibrations)
            .FirstOrDefaultAsync(e => e.Id == id, ct);


    public Task<List<Equipment>> GetAllAsync(CancellationToken ct = default)
        => _db.Equipments.AsNoTracking().ToListAsync(ct);


    public async Task AddAsync(Equipment equipment, CancellationToken ct = default)
    => await _db.Equipments.AddAsync(equipment, ct);


    public Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null, CancellationToken ct = default)
        => _db.Equipments
            .AnyAsync(e => e.Barcode == barcode && (excludeId == null || e.Id != excludeId), ct);


    public Task<CalibrationRecord?> GetCalibrationAsync(int equipmentId, int calibrationId, CancellationToken ct = default)
        => _db.CalibrationRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.EquipmentId == equipmentId 
                                            && c.Id == calibrationId, ct);


    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

}