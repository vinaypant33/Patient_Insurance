using PatientInsurance.ML;
using Xunit;

namespace PatientInsurance.Tests;

/// <summary>Trains one model on dummy data and shares it between the tests.</summary>
public class TrainedModelFixture
{
    public PremiumModel Model { get; } = new();
    public ModelMetrics Metrics { get; }

    public TrainedModelFixture()
    {
        Metrics = Model.Train(new SyntheticDataGenerator(42).Generate(1338));
    }
}
