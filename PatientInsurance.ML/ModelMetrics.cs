namespace PatientInsurance.ML;

/// <summary>How well the model did on test rows it never saw while training.</summary>
public record ModelMetrics(
    double RSquared,
    double MeanAbsoluteError,
    double RootMeanSquaredError,
    int TrainingRows,
    int TestRows)
{
    /// <summary>R² as a percentage, e.g. 86.4 means the model explains 86.4% of the variation.</summary>
    public double RSquaredPercent => Math.Round(RSquared * 100, 1);
}
