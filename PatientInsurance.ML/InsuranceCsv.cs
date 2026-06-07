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

    public static List<InsuranceRecord> Read(string path)
    {
        var rows = new List<InsuranceRecord>();
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0 || lines[0].Trim() != Header)
            throw new InvalidDataException($"{path} does not start with the header '{Header}'");

        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var parts = lines[i].Split(',');
            if (parts.Length != 7)
                throw new InvalidDataException($"{path} line {i + 1}: expected 7 values, got {parts.Length}");

            rows.Add(new InsuranceRecord
            {
                Age = float.Parse(parts[0], CultureInfo.InvariantCulture),
                Sex = parts[1],
                Bmi = float.Parse(parts[2], CultureInfo.InvariantCulture),
                Children = float.Parse(parts[3], CultureInfo.InvariantCulture),
                Smoker = parts[4],
                Region = parts[5],
                Charges = float.Parse(parts[6], CultureInfo.InvariantCulture),
            });
        }
        return rows;
    }
}
