namespace PatientInsurance.ML;

public enum RiskLevel
{
    Low,
    Medium,
    High,
}

/// <summary>Turns a predicted yearly cost into a risk level and a premium.</summary>
public static class RiskClassifier
{
    /// <summary>Yearly cost below this is Low risk.</summary>
    public const decimal MediumThreshold = 10_000m;

    /// <summary>Yearly cost at or above this is High risk.</summary>
    public const decimal HighThreshold = 25_000m;

    /// <summary>Extra charged on top of the expected cost to cover running costs.</summary>
    public const decimal PremiumLoading = 0.15m;

    public static RiskLevel Classify(decimal predictedAnnualCost)
    {
        if (predictedAnnualCost >= HighThreshold) return RiskLevel.High;
        if (predictedAnnualCost >= MediumThreshold) return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    /// <summary>Suggested monthly premium: expected yearly cost plus loading, over 12 months.</summary>
    public static decimal MonthlyPremium(decimal predictedAnnualCost)
    {
        var yearly = Math.Max(0m, predictedAnnualCost) * (1m + PremiumLoading);
        return Math.Round(yearly / 12m, 0, MidpointRounding.AwayFromZero);
    }
}
