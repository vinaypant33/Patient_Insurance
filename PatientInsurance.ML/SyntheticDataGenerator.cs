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

    private int NextChildren()
    {
        // Roughly how family sizes are spread in the real data.
        var roll = _random.NextDouble();
        if (roll < 0.43) return 0;
        if (roll < 0.67) return 1;
        if (roll < 0.85) return 2;
        if (roll < 0.97) return 3;
        if (roll < 0.99) return 4;
        return 5;
    }

    private double Charges(int age, double bmi, int children, bool smoker, string region)
    {
        double cost = -2300 + 265 * age + 480 * children;

        cost += region switch
        {
            "northwest" => -350,
            "southeast" => -1000,
            "southwest" => -950,
            _ => 0,
        };

        if (smoker)
        {
            cost += 13500 + 450 * (bmi - 30);
            if (bmi >= 30) cost += 18500; // smoking and obesity together cost far more
        }
        else
        {
            cost += 20 * (bmi - 30);
            if (_random.NextDouble() < 0.06) cost += 6000 + 12000 * _random.NextDouble(); // a few expensive cases
        }

        cost += NextGaussian(0, 1400);
        return Math.Max(1100, cost);
    }

    private double NextGaussian(double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = 1.0 - _random.NextDouble();
        var u2 = _random.NextDouble();
        var standard = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * standard;
    }
}
