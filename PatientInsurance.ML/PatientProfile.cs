namespace PatientInsurance.ML;

/// <summary>
/// The details the app knows about a patient, in friendly types.
/// Converted to an <see cref="InsuranceRecord"/> before it goes to the model.
/// </summary>
public record PatientProfile(int Age, bool IsMale, double Bmi, int Children, bool IsSmoker, string Region)
{
    public static readonly string[] Regions = { "northeast", "northwest", "southeast", "southwest" };
}
