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
}
