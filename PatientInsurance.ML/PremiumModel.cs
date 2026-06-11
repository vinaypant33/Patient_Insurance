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

    /// <summary>
    /// Trains on 80% of the rows and measures accuracy on the other 20%.
    /// </summary>
    public ModelMetrics Train(IReadOnlyCollection<InsuranceRecord> rows, double testFraction = 0.2)
    {
        if (rows.Count < 20) throw new ArgumentException("Need at least 20 rows to train", nameof(rows));

        var data = _ml.Data.LoadFromEnumerable(rows);
        var split = _ml.Data.TrainTestSplit(data, testFraction, seed: 7);

        var model = BuildPipeline().Fit(split.TrainSet);
        var scored = model.Transform(split.TestSet);
        var result = _ml.Regression.Evaluate(scored, labelColumnName: Label);

        var testRows = split.TestSet.GetColumn<float>(Label).Count();
        Metrics = new ModelMetrics(
            result.RSquared,
            result.MeanAbsoluteError,
            result.RootMeanSquaredError,
            rows.Count - testRows,
            testRows);

        lock (_predictLock)
        {
            _model = model;
            _engine = _ml.Model.CreatePredictionEngine<InsuranceRecord, CostPrediction>(model);
        }
        return Metrics;
    }

    public ModelMetrics TrainFromCsv(string path) => Train(InsuranceCsv.Read(path));

    /// <summary>Predicted yearly medical charges in dollars.</summary>
    public decimal PredictAnnualCost(PatientProfile profile)
    {
        var problems = profile.Validate();
        if (problems.Count > 0) throw new ArgumentException(string.Join(" ", problems), nameof(profile));

        lock (_predictLock)
        {
            if (_engine == null) throw new InvalidOperationException("Train the model before predicting.");
            var prediction = _engine.Predict(profile.ToRecord());
            return Math.Max(0m, Math.Round((decimal)prediction.PredictedCharges, 2));
        }
    }
}
