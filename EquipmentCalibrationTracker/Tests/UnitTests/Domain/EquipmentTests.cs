using Domain.Entities;

namespace UnitTests.Domain;


public class EquipmentTests
{

    [Fact]
    public void IsDue_WhenNextDueIsNull_ReturnsFalse()
    {
        // Arrange
        var equipment = new Equipment { NextDueDate = null };

        // Act
        var result = equipment.IsDue(new DateTime(2026, 3, 15));

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsDue_WhenNextDueDateIsInThePast_ReturnsTrue()
    {
        var equipment = new Equipment { NextDueDate = new DateTime(2026, 3, 1) };

        var result = equipment.IsDue(new DateTime(2026, 3, 15));

        Assert.True(result);
    }


    [Theory]    
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(30, false)]
    public void IsDue_WhenActive_DependsOnNextDueDate(int daysFromToday, bool expected)
    {
        var today = new DateTime(2026, 3, 15);
        var equipment = new Equipment { NextDueDate = today.AddDays(daysFromToday) };

        var result = equipment.IsDue(today);

        Assert.Equal(expected, result);
    }
}
