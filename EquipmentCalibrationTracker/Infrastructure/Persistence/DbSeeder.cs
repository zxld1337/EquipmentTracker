using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence;

public static class DbSeeder
{

    public static void Seed(AppDbContext db)
    {
        if (!db.Equipments.Any())
        {
            var torque = new Equipment
            {
                Name = "Torque wrench A",
                Barcode = "EQ-001",
                Location = "Hatvan",
                CalibrationIntervalMonths = 12,
                Calibrations = new List<CalibrationRecord>
        {
            new() { PerformedOn = DateTime.Today.AddMonths(-9), PerformedBy = "Kovacs I.", Result = CalibrationResult.Pass },
            new() { PerformedOn = DateTime.Today.AddMonths(-21), PerformedBy = "Kovacs I.", Result = CalibrationResult.Pass }
        }
            };
            torque.NextDueDate = DateTime.Today.AddMonths(3);

            var caliper = new Equipment
            {
                Name = "Caliper B",
                Barcode = "EQ-002",
                Location = "Hatvan",
                CalibrationIntervalMonths = 6,
                Calibrations = new List<CalibrationRecord>
        {
            new() { PerformedOn = DateTime.Today.AddMonths(-7), PerformedBy = "Nagy P.", Result = CalibrationResult.Fail, Notes = "Out of tolerance" }
        }
            };
            caliper.NextDueDate = DateTime.Today.AddDays(-10);

            var multimeter = new Equipment
            {
                Name = "Multimeter C",
                Barcode = "EQ-003",
                Location = "Budapest",
                CalibrationIntervalMonths = 24
            };

            db.Equipments.AddRange(torque, caliper, multimeter);
            db.SaveChanges();
        }
    }
        
}
