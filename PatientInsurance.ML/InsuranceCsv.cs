using System.Globalization;
using System.Text;

namespace PatientInsurance.ML;

/// <summary>Reads and writes the insurance CSV file.</summary>
public static class InsuranceCsv
{
    public const string Header = "age,sex,bmi,children,smoker,region,charges";
}
