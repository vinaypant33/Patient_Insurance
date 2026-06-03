using Microsoft.ML.Data;

namespace PatientInsurance.ML;

/// <summary>
/// One row of the insurance dataset: a person's details and their yearly
/// medical charges. Column order matches data/insurance_dummy.csv.
/// </summary>
public class InsuranceRecord
{
    [LoadColumn(0)]
    public float Age { get; set; }

    [LoadColumn(1)]
    public string Sex { get; set; } = "female";

    [LoadColumn(2)]
    public float Bmi { get; set; }

    [LoadColumn(3)]
    public float Children { get; set; }

    [LoadColumn(4)]
    public string Smoker { get; set; } = "no";

    [LoadColumn(5)]
    public string Region { get; set; } = "northeast";

    /// <summary>Yearly medical charges in dollars. This is what the model predicts.</summary>
    [LoadColumn(6)]
    public float Charges { get; set; }
}

/// <summary>The model's output for one person.</summary>
public class CostPrediction
{
    /// <summary>Predicted yearly medical charges in dollars.</summary>
    [ColumnName("Score")]
    public float PredictedCharges { get; set; }
}
