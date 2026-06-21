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

    [Fact]
    public void Model_explains_most_of_the_variation()
    {
        Assert.True(_fixture.Metrics.RSquared > 0.8, _fixture.Metrics.Summary);
    }

    [Fact]
    public void Split_uses_about_a_fifth_for_testing()
    {
        var metrics = _fixture.Metrics;
        Assert.Equal(1338, metrics.TrainingRows + metrics.TestRows);
        Assert.InRange(metrics.TestRows, 200, 340);
    }

    [Fact]
    public void Smoker_costs_more_than_non_smoker()
    {
        var smoker = _fixture.Model.PredictAnnualCost(Profile(smoker: true));
        var nonSmoker = _fixture.Model.PredictAnnualCost(Profile(smoker: false));
        Assert.True(smoker > nonSmoker * 2, $"{smoker} vs {nonSmoker}");
    }

    [Fact]
    public void Older_patient_costs_more()
    {
        var young = _fixture.Model.PredictAnnualCost(Profile(age: 22));
        var old = _fixture.Model.PredictAnnualCost(Profile(age: 62));
        Assert.True(old > young, $"{old} vs {young}");
    }
}
