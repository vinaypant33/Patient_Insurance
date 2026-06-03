namespace PatientInsurance.ML;

/// <summary>
/// The details the app knows about a patient, in friendly types.
/// Converted to an <see cref="InsuranceRecord"/> before it goes to the model.
/// </summary>
public record PatientProfile(int Age, bool IsMale, double Bmi, int Children, bool IsSmoker, string Region)
{
    public static readonly string[] Regions = { "northeast", "northwest", "southeast", "southwest" };

    /// <summary>Returns a list of problems with the input, empty when it is valid.</summary>
    public IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();
        if (Age < 18 || Age > 100) problems.Add("Age must be between 18 and 100.");
        if (Bmi < 10 || Bmi > 70) problems.Add("BMI must be between 10 and 70.");
        if (Children < 0 || Children > 10) problems.Add("Children must be between 0 and 10.");
        if (!Regions.Contains(Region)) problems.Add($"Region must be one of: {string.Join(", ", Regions)}.");
        return problems;
    }
}
