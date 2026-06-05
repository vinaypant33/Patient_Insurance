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
}
