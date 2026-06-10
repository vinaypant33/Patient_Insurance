using Microsoft.ML;
using Microsoft.ML.Data;

namespace PatientInsurance.ML;

/// <summary>
/// Predicts a person's yearly medical charges with ML.NET.
///
/// The text columns (sex, smoker, region) are one-hot encoded, joined with the
/// number columns into one feature vector, and fed to a FastTree regression
/// model (gradient boosted decision trees). Trees handle the big jump in cost
/// for smokers with a high BMI much better than a straight-line model.
/// </summary>
public class PremiumModel
{
    private const string Label = nameof(InsuranceRecord.Charges);

    private readonly MLContext _ml;
    private readonly object _predictLock = new();
    private ITransformer? _model;
    private PredictionEngine<InsuranceRecord, CostPrediction>? _engine;

    public PremiumModel(int seed = 7)
    {
        _ml = new MLContext(seed);
    }

    public bool IsTrained => _model != null;

    public ModelMetrics? Metrics { get; private set; }

    private IEstimator<ITransformer> BuildPipeline()
    {
        return _ml.Transforms.Categorical.OneHotEncoding(new[]
            {
                new InputOutputColumnPair("SexEncoded", nameof(InsuranceRecord.Sex)),
                new InputOutputColumnPair("SmokerEncoded", nameof(InsuranceRecord.Smoker)),
                new InputOutputColumnPair("RegionEncoded", nameof(InsuranceRecord.Region)),
            })
            .Append(_ml.Transforms.Concatenate("Features",
                nameof(InsuranceRecord.Age),
                nameof(InsuranceRecord.Bmi),
                nameof(InsuranceRecord.Children),
                "SexEncoded",
                "SmokerEncoded",
                "RegionEncoded"))
            .Append(_ml.Regression.Trainers.FastTree(
                labelColumnName: Label,
                featureColumnName: "Features",
                numberOfLeaves: 16,
                numberOfTrees: 200,
                minimumExampleCountPerLeaf: 10,
                learningRate: 0.1));
    }
}
