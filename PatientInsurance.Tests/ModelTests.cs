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

public class ModelTests : IClassFixture<TrainedModelFixture>
{
    private readonly TrainedModelFixture _fixture;

    public ModelTests(TrainedModelFixture fixture)
    {
        _fixture = fixture;
    }

    private static PatientProfile Profile(int age = 40, double bmi = 27, bool smoker = false, int children = 1) =>
        new(age, IsMale: true, bmi, children, smoker, "northeast");
}
