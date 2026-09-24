
using Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<CalibrationRecord> CalibrationRecords => Set<CalibrationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>()
            .HasQueryFilter(e => e.DeletedAt == null);

        modelBuilder.Entity<Equipment>()
            .HasIndex(e => e.Barcode)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
