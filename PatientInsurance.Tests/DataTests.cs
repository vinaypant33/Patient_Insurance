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
}
