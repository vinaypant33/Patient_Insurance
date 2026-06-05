namespace PatientInsurance.ML;

/// <summary>
/// Creates realistic dummy insurance data. No real people are involved: every
/// row is random, but the charges follow the same patterns as real medical
/// cost data (older, smoking and high-BMI people cost more).
/// The same seed always gives the same rows.
/// </summary>
public class SyntheticDataGenerator
{
    private readonly Random _random;

    public SyntheticDataGenerator(int seed = 42)
    {
        _random = new Random(seed);
    }

    public List<InsuranceRecord> Generate(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "count must be positive");

        var rows = new List<InsuranceRecord>(count);
        for (var i = 0; i < count; i++)
        {
            rows.Add(NextRecord());
        }
        return rows;
    }

    private InsuranceRecord NextRecord()
    {
        var age = _random.Next(18, 65);
        var isMale = _random.NextDouble() < 0.5;
        var region = PatientProfile.Regions[_random.Next(PatientProfile.Regions.Length)];
        var bmiShift = region == "southeast" ? 1.5 : 0.0;
        var bmi = Math.Clamp(NextGaussian(30.4 + bmiShift, 6.0), 16.0, 53.0);
        var children = NextChildren();
        var smoker = _random.NextDouble() < 0.205;

        return new InsuranceRecord
        {
            Age = age,
            Sex = isMale ? "male" : "female",
            Bmi = (float)Math.Round(bmi, 2),
            Children = children,
            Smoker = smoker ? "yes" : "no",
            Region = region,
            Charges = (float)Math.Round(Charges(age, bmi, children, smoker, region), 2),
        };
    }
}
