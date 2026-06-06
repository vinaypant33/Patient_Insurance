using System.Globalization;
using System.Text;

namespace PatientInsurance.ML;

/// <summary>Reads and writes the insurance CSV file.</summary>
public static class InsuranceCsv
{
    public const string Header = "age,sex,bmi,children,smoker,region,charges";

    public static void Write(string path, IEnumerable<InsuranceRecord> rows)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

        var text = new StringBuilder();
        text.Append(Header).Append('\n');
        foreach (var row in rows)
        {
            text.Append(string.Join(",",
                row.Age.ToString(CultureInfo.InvariantCulture),
                row.Sex,
                row.Bmi.ToString("0.##", CultureInfo.InvariantCulture),
                row.Children.ToString(CultureInfo.InvariantCulture),
                row.Smoker,
                row.Region,
                row.Charges.ToString("0.##", CultureInfo.InvariantCulture)));
            text.Append('\n');
        }
        File.WriteAllText(path, text.ToString());
    }
}
