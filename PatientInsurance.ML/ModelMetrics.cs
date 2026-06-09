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

    public string Summary =>
        $"R² {RSquared:0.000}, average error ${MeanAbsoluteError:N0}, RMSE ${RootMeanSquaredError:N0} " +
        $"({TrainingRows} training rows, {TestRows} test rows)";
}
