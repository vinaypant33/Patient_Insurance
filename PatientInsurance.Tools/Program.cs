using System.Globalization;
using PatientInsurance.ML;

namespace PatientInsurance.Tools;

/// <summary>
/// Command line helpers for the dummy data and the model.
///
///   dotnet run --project PatientInsurance.Tools -- generate [rows] [seed] [output.csv]
///   dotnet run --project PatientInsurance.Tools -- train [input.csv] [model.zip]
/// </summary>
public static class Program
{
    private const string DefaultCsv = "data/insurance_dummy.csv";
    private const string DefaultModel = "data/premium_model.zip";

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            return args[0] switch
            {
                "generate" => Generate(args),
                "train" => Train(args),
                _ => Unknown(args[0]),
            };
        }
        catch (Exception error) when (error is IOException or InvalidDataException or ArgumentException or FormatException)
        {
            Console.Error.WriteLine($"error: {error.Message}");
            return 1;
        }
    }

    private static int Generate(string[] args)
    {
        var rows = args.Length > 1 ? int.Parse(args[1], CultureInfo.InvariantCulture) : 1338;
        var seed = args.Length > 2 ? int.Parse(args[2], CultureInfo.InvariantCulture) : 42;
        var output = args.Length > 3 ? args[3] : DefaultCsv;

        var records = new SyntheticDataGenerator(seed).Generate(rows);
        InsuranceCsv.Write(output, records);

        var smokers = records.Count(r => r.Smoker == "yes");
        Console.WriteLine($"Wrote {records.Count} dummy rows to {output} (seed {seed})");
        Console.WriteLine($"  smokers: {smokers} ({100.0 * smokers / records.Count:0.0}%)");
        Console.WriteLine($"  average charges: ${records.Average(r => r.Charges):N0}");
        return 0;
    }
}
