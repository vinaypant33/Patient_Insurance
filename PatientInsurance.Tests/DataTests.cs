using PatientInsurance.ML;
using Xunit;

namespace PatientInsurance.Tests;

public class DataTests
{
    [Fact]
    public void Generator_returns_requested_number_of_rows()
    {
        var rows = new SyntheticDataGenerator(1).Generate(250);
        Assert.Equal(250, rows.Count);
    }

    [Fact]
    public void Same_seed_gives_same_data()
    {
        var first = new SyntheticDataGenerator(5).Generate(50);
        var second = new SyntheticDataGenerator(5).Generate(50);
        Assert.Equal(first.Select(r => r.Charges), second.Select(r => r.Charges));
    }

    [Fact]
    public void Values_stay_in_realistic_ranges()
    {
        var rows = new SyntheticDataGenerator(3).Generate(2000);
        Assert.All(rows, r =>
        {
            Assert.InRange(r.Age, 18, 64);
            Assert.InRange(r.Bmi, 16, 53);
            Assert.InRange(r.Children, 0, 5);
            Assert.Contains(r.Sex, new[] { "male", "female" });
            Assert.Contains(r.Smoker, new[] { "yes", "no" });
            Assert.Contains(r.Region, PatientProfile.Regions);
            Assert.True(r.Charges >= 1100);
        });
    }
}
