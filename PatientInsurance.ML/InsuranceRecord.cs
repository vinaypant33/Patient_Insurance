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
}
