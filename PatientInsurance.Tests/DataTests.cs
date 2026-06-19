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

    [Fact]
    public void About_a_fifth_are_smokers()
    {
        var rows = new SyntheticDataGenerator(9).Generate(5000);
        var share = rows.Count(r => r.Smoker == "yes") / (double)rows.Count;
        Assert.InRange(share, 0.17, 0.24);
    }

    [Fact]
    public void Smokers_cost_more_on_average()
    {
        var rows = new SyntheticDataGenerator(11).Generate(3000);
        var smokers = rows.Where(r => r.Smoker == "yes").Average(r => r.Charges);
        var nonSmokers = rows.Where(r => r.Smoker == "no").Average(r => r.Charges);
        Assert.True(smokers > nonSmokers * 2.5, $"smokers {smokers:N0} vs non-smokers {nonSmokers:N0}");
    }

    [Fact]
    public void Csv_round_trip_keeps_values()
    {
        var path = Path.Combine(Path.GetTempPath(), $"insurance_{Guid.NewGuid():N}.csv");
        try
        {
            var rows = new SyntheticDataGenerator(2).Generate(30);
            InsuranceCsv.Write(path, rows);
            var back = InsuranceCsv.Read(path);

            Assert.Equal(rows.Count, back.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                Assert.Equal(rows[i].Age, back[i].Age);
                Assert.Equal(rows[i].Region, back[i].Region);
                Assert.Equal(rows[i].Charges, back[i].Charges, 2);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Csv_with_wrong_header_is_rejected()
    {
        var path = Path.Combine(Path.GetTempPath(), $"bad_{Guid.NewGuid():N}.csv");
        try
        {
            File.WriteAllText(path, "name,amount\nx,1\n");
            Assert.Throws<InvalidDataException>(() => InsuranceCsv.Read(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
