using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PatientInsurance.ML;

namespace PatientInsurance;

// ============================================================================
// Models
// ============================================================================

public enum ClaimStatus
{
    Pending,
    UnderReview,
    Approved,
    Denied
}

public class InsurancePolicy
{
    public required string PolicyNumber { get; init; }
    public required string PlanName { get; init; }
    public decimal MonthlyPremium { get; init; }
    public DateTime RenewalDate { get; init; }
    public double CoverageUsedPercent { get; init; }
    public double CoverageUsedPercentDisplay => CoverageUsedPercent * 100;
}

public class Claim
{
    public required string Id { get; init; }
    public required string PatientName { get; init; }
    public required string ClaimType { get; init; }
    public decimal Amount { get; init; }
    public ClaimStatus Status { get; init; }
    public DateTime SubmittedOn { get; init; }
}

public class PatientRecord : ObservableObject
{
    private decimal? _predictedAnnualCost;
    private RiskLevel? _risk;

    public required string Name { get; init; }
    public required string PolicyNumber { get; init; }
    public int Age { get; init; }
    public required string Status { get; init; }

    // Health details used by the premium model
    public bool IsMale { get; init; }
    public double Bmi { get; init; }
    public int Children { get; init; }
    public bool IsSmoker { get; init; }
    public string Region { get; init; } = "northeast";

    public string SmokerDisplay => IsSmoker ? "Yes" : "No";
    public string RegionDisplay => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Region);

    /// <summary>Filled in once the model has finished training.</summary>
    public decimal? PredictedAnnualCost
    {
        get => _predictedAnnualCost;
        set => SetProperty(ref _predictedAnnualCost, value);
    }

    public RiskLevel? Risk
    {
        get => _risk;
        set => SetProperty(ref _risk, value);
    }

    public PatientProfile ToProfile() => new(Age, IsMale, Bmi, Children, IsSmoker, Region);
}

// ============================================================================
// Mock data service
// ============================================================================

public class DashboardDataService
{
    public int TotalPatients => 1284;
    public int ActivePolicies => 947;
    public int PendingClaims => 36;
    public decimal PremiumCollectedThisMonth => 128450m;

    public List<Claim> GetRecentClaims() => new()
    {
        new Claim { Id = "CLM-1042", PatientName = "Ava Thompson", ClaimType = "Dental", Amount = 320m, Status = ClaimStatus.Approved, SubmittedOn = DateTime.Now.AddDays(-1) },
        new Claim { Id = "CLM-1041", PatientName = "Liam Ortiz", ClaimType = "Outpatient Surgery", Amount = 4200m, Status = ClaimStatus.UnderReview, SubmittedOn = DateTime.Now.AddDays(-2) },
        new Claim { Id = "CLM-1039", PatientName = "Sofia Kim", ClaimType = "Prescription", Amount = 85m, Status = ClaimStatus.Approved, SubmittedOn = DateTime.Now.AddDays(-2) },
        new Claim { Id = "CLM-1035", PatientName = "Noah Bennett", ClaimType = "Physical Therapy", Amount = 610m, Status = ClaimStatus.Pending, SubmittedOn = DateTime.Now.AddDays(-3) },
        new Claim { Id = "CLM-1030", PatientName = "Emma Rossi", ClaimType = "MRI Scan", Amount = 1450m, Status = ClaimStatus.Denied, SubmittedOn = DateTime.Now.AddDays(-4) },
    };

    public List<InsurancePolicy> GetUpcomingRenewals() => new()
    {
        new InsurancePolicy { PolicyNumber = "POL-3391", PlanName = "Family Plus Care", MonthlyPremium = 410m, RenewalDate = DateTime.Now.AddDays(5), CoverageUsedPercent = 0.62 },
        new InsurancePolicy { PolicyNumber = "POL-2287", PlanName = "Essential Health", MonthlyPremium = 190m, RenewalDate = DateTime.Now.AddDays(9), CoverageUsedPercent = 0.34 },
        new InsurancePolicy { PolicyNumber = "POL-4410", PlanName = "Senior Complete", MonthlyPremium = 275m, RenewalDate = DateTime.Now.AddDays(14), CoverageUsedPercent = 0.81 },
    };

    public Dictionary<ClaimStatus, int> GetClaimStatusBreakdown() => new()
    {
        { ClaimStatus.Approved, 62 },
        { ClaimStatus.Pending, 18 },
        { ClaimStatus.UnderReview, 14 },
        { ClaimStatus.Denied, 6 },
    };

    public List<PatientRecord> GetPatients() => new()
    {
        new PatientRecord { Name = "Ava Thompson", PolicyNumber = "POL-3391", Age = 34, Status = "Active", Bmi = 24.1, Children = 2, IsSmoker = false, Region = "northeast" },
        new PatientRecord { Name = "Liam Ortiz", PolicyNumber = "POL-2287", Age = 58, Status = "Active", IsMale = true, Bmi = 31.8, Children = 0, IsSmoker = true, Region = "southwest" },
        new PatientRecord { Name = "Sofia Kim", PolicyNumber = "POL-4410", Age = 27, Status = "Active", Bmi = 21.5, Children = 0, IsSmoker = false, Region = "northwest" },
        new PatientRecord { Name = "Noah Bennett", PolicyNumber = "POL-5521", Age = 45, Status = "Inactive", IsMale = true, Bmi = 28.9, Children = 3, IsSmoker = false, Region = "southeast" },
        new PatientRecord { Name = "Emma Rossi", PolicyNumber = "POL-6634", Age = 61, Status = "Active", Bmi = 33.4, Children = 1, IsSmoker = false, Region = "northeast" },
        new PatientRecord { Name = "Mason Clarke", PolicyNumber = "POL-7788", Age = 39, Status = "Pending", IsMale = true, Bmi = 26.7, Children = 2, IsSmoker = true, Region = "southeast" },
        new PatientRecord { Name = "Isabella Grant", PolicyNumber = "POL-8102", Age = 52, Status = "Active", Bmi = 29.3, Children = 2, IsSmoker = false, Region = "northwest" },
        new PatientRecord { Name = "Ethan Walsh", PolicyNumber = "POL-8240", Age = 23, Status = "Active", IsMale = true, Bmi = 35.2, Children = 0, IsSmoker = true, Region = "southwest" },
    };
}

// ============================================================================
// View model
// ============================================================================

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public class DashboardViewModel : ObservableObject
{
    private readonly DashboardDataService _dataService = new();

    public int TotalPatients { get; }
    public int ActivePolicies { get; }
    public int PendingClaims { get; }
    public decimal PremiumCollected { get; }
    public string PremiumCollectedDisplay => PremiumCollected.ToString("C0");

    public ObservableCollection<Claim> RecentClaims { get; }
    public ObservableCollection<InsurancePolicy> UpcomingRenewals { get; }
    public Dictionary<ClaimStatus, int> ClaimStatusBreakdown { get; }

    public DashboardViewModel()
    {
        TotalPatients = _dataService.TotalPatients;
        ActivePolicies = _dataService.ActivePolicies;
        PendingClaims = _dataService.PendingClaims;
        PremiumCollected = _dataService.PremiumCollectedThisMonth;

        RecentClaims = new ObservableCollection<Claim>(_dataService.GetRecentClaims());
        UpcomingRenewals = new ObservableCollection<InsurancePolicy>(_dataService.GetUpcomingRenewals());
        ClaimStatusBreakdown = _dataService.GetClaimStatusBreakdown();
    }
}

// ============================================================================
// Converter (colors are self-contained, no dependency on App-level resources)
// ============================================================================

public class ClaimStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Success = new(Color.FromRgb(46, 158, 91));
    private static readonly SolidColorBrush Warning = new(Color.FromRgb(224, 167, 46));
    private static readonly SolidColorBrush Info = new(Color.FromRgb(62, 124, 219));
    private static readonly SolidColorBrush Danger = new(Color.FromRgb(213, 84, 74));
    private static readonly SolidColorBrush Navy = new(Color.FromRgb(21, 34, 56));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ClaimStatus status ? status switch
        {
            ClaimStatus.Approved => Success,
            ClaimStatus.Pending => Warning,
            ClaimStatus.UnderReview => Info,
            ClaimStatus.Denied => Danger,
            _ => Navy
        } : Navy;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class RiskToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Low = new(Color.FromRgb(46, 158, 91));
    private static readonly SolidColorBrush Medium = new(Color.FromRgb(224, 167, 46));
    private static readonly SolidColorBrush High = new(Color.FromRgb(213, 84, 74));
    private static readonly SolidColorBrush Unknown = new(Color.FromRgb(154, 164, 178));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is RiskLevel risk ? risk switch
        {
            RiskLevel.Low => Low,
            RiskLevel.Medium => Medium,
            RiskLevel.High => High,
            _ => Unknown
        } : Unknown;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

// ============================================================================
// MainWindow — layout is in MainWindow.xaml, all behavior lives here
// ============================================================================

public partial class MainWindow : Window
{
    private readonly DashboardViewModel _viewModel = new();
    private readonly DashboardDataService _dataService = new();
    private bool _isDarkTheme;
    private bool _sidebarOpen = true;
    private string _currentPage = "Dashboard";

    private readonly Dictionary<ClaimStatus, Color> _palette = new()
    {
        { ClaimStatus.Approved, Color.FromArgb(255, 46, 158, 91) },
        { ClaimStatus.Pending, Color.FromArgb(255, 224, 167, 46) },
        { ClaimStatus.UnderReview, Color.FromArgb(255, 62, 124, 219) },
        { ClaimStatus.Denied, Color.FromArgb(255, 213, 84, 74) },
    };

    public MainWindow()
    {
        InitializeComponent();

        RootGrid.DataContext = _viewModel;
        PatientsGrid.ItemsSource = _dataService.GetPatients();

        ApplyTheme(dark: false);
        ShowPage("Dashboard");

        RootGrid.Loaded += OnRootLoaded;
    }

    // ---- navigation ----

    private void NavButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string key })
        {
            ShowPage(key);
        }
    }

    private void ShowPage(string key)
    {
        _currentPage = key;

        DashboardPage.Visibility = key == "Dashboard" ? Visibility.Visible : Visibility.Collapsed;
        PatientsPage.Visibility = key == "Patients" ? Visibility.Visible : Visibility.Collapsed;
        SubmissionsPage.Visibility = key == "Submissions" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPage.Visibility = key == "Settings" ? Visibility.Visible : Visibility.Collapsed;

        PageTitleText.Text = key switch
        {
            "Dashboard" => "Dashboard",
            "Patients" => "Patient Data",
            "Submissions" => "Current Submissions",
            "Settings" => "Settings",
            _ => "Dashboard"
        };

        SetNavSelected(key);
    }

    private void SetNavSelected(string key)
    {
        var selected = (Brush)Resources["NavSelectedBrush"];

        NavDashboardButton.Background = key == "Dashboard" ? selected : Brushes.Transparent;
        NavPatientsButton.Background = key == "Patients" ? selected : Brushes.Transparent;
        NavSubmissionsButton.Background = key == "Submissions" ? selected : Brushes.Transparent;
        NavSettingsButton.Background = key == "Settings" ? selected : Brushes.Transparent;
    }

    // ---- sidebar collapse/expand ----

    private void HamburgerButton_Click(object sender, RoutedEventArgs e)
    {
        _sidebarOpen = !_sidebarOpen;
        var targetWidth = _sidebarOpen ? 230.0 : 0.0;

        if (_sidebarOpen)
        {
            SidebarContent.Visibility = Visibility.Visible;
        }

        var anim = new DoubleAnimation
        {
            To = targetWidth,
            Duration = new Duration(TimeSpan.FromMilliseconds(220)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        if (!_sidebarOpen)
        {
            anim.Completed += (s, e2) => SidebarContent.Visibility = Visibility.Collapsed;
        }

        SidebarBorder.BeginAnimation(FrameworkElement.WidthProperty, anim);
    }

    // ---- theme ----

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string theme })
        {
            ApplyTheme(dark: theme == "Dark");
        }
    }

    private void ApplyTheme(bool dark)
    {
        _isDarkTheme = dark;

        SetBrush("WindowBackgroundBrush", dark ? "#12161C" : "#F4F7F9");
        SetBrush("SurfaceBrush", dark ? "#1C222B" : "#FFFFFF");
        SetBrush("PrimaryTextBrush", dark ? "#F0F3F7" : "#152238");
        SetBrush("SecondaryTextBrush", dark ? "#9AA4B2" : "#6B7280");
        SetBrush("CardBorderBrush", dark ? "#30FFFFFF" : "#12000000");
        SetBrush("InputBackgroundBrush", dark ? "#232A34" : "#F1F3F5");
        SetBrush("GridHeaderBackgroundBrush", dark ? "#1A2028" : "#F7F9FA");
        SetBrush("GridAlternateRowBrush", dark ? "#1F252E" : "#FAFBFC");
        SetBrush("GridRowHoverBrush", dark ? "#303E7CDB" : "#143E7CDB");
        SetBrush("GridLineBrush", dark ? "#20FFFFFF" : "#14000000");
        SetBrush("NavSelectedBrush", dark ? "#403E7CDB" : "#1F3E7CDB");

        UpdateThemeButtons();
        SetNavSelected(_currentPage);
    }

    private void SetBrush(string key, string hex)
    {
        // Replace (not mutate) the resource: brushes referenced inside Style Setters get
        // frozen by WPF for sharing, so assigning a fresh brush is what makes DynamicResource
        // consumers pick up the change.
        Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
    }

    private void UpdateThemeButtons()
    {
        var accent = (Brush)Resources["InfoBrush"];
        var neutral = (Brush)Resources["CardBorderBrush"];

        LightThemeButton.BorderBrush = !_isDarkTheme ? accent : neutral;
        DarkThemeButton.BorderBrush = _isDarkTheme ? accent : neutral;
    }

    // ---- dashboard load ----

    private void OnRootLoaded(object sender, RoutedEventArgs e)
    {
        PremiumValueText.Text = _viewModel.PremiumCollectedDisplay;

        AnimateEntrance(StatsGrid, 0);
        AnimateEntrance(ContentGrid, 100);

        var startTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        startTimer.Tick += (s, e2) =>
        {
            startTimer.Stop();
            AnimateCount(PatientsValueText, 0, _viewModel.TotalPatients);
            AnimateCount(PoliciesValueText, 0, _viewModel.ActivePolicies);
            AnimateCount(ClaimsValueText, 0, _viewModel.PendingClaims);
            AnimateDonutChart();
        };
        startTimer.Start();
    }

    private static void AnimateCount(TextBlock target, int from, int to, int durationMs = 900)
    {
        const int steps = 30;
        var current = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs / (double)steps) };
        timer.Tick += (s, e) =>
        {
            current++;
            var value = from + (to - from) * current / steps;
            target.Text = value.ToString("N0");
            if (current >= steps)
            {
                target.Text = to.ToString("N0");
                ((DispatcherTimer)s!).Stop();
            }
        };
        timer.Start();
    }

    private void OnCardMouseEnter(object sender, MouseEventArgs e) => AnimateCardScale(sender, 1.03);

    private void OnCardMouseLeave(object sender, MouseEventArgs e) => AnimateCardScale(sender, 1.0);

    private static void AnimateCardScale(object sender, double to)
    {
        if (sender is not Border border || border.RenderTransform is not ScaleTransform scale) return;

        var sb = new Storyboard();
        var duration = new Duration(TimeSpan.FromMilliseconds(180));
        var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

        var xAnim = new DoubleAnimation { To = to, Duration = duration, EasingFunction = easing };
        var yAnim = new DoubleAnimation { To = to, Duration = duration, EasingFunction = easing };

        Storyboard.SetTarget(xAnim, scale);
        Storyboard.SetTargetProperty(xAnim, new PropertyPath(ScaleTransform.ScaleXProperty));
        Storyboard.SetTarget(yAnim, scale);
        Storyboard.SetTargetProperty(yAnim, new PropertyPath(ScaleTransform.ScaleYProperty));

        sb.Children.Add(xAnim);
        sb.Children.Add(yAnim);
        sb.Begin();
    }

    private static void AnimateEntrance(UIElement element, int delayMs)
    {
        element.Opacity = 0;
        var transform = new TranslateTransform { Y = 24 };
        element.RenderTransform = transform;

        var sb = new Storyboard();
        var fade = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(420)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var slide = new DoubleAnimation
        {
            From = 24,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(420)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(fade, element);
        Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
        Storyboard.SetTarget(slide, transform);
        Storyboard.SetTargetProperty(slide, new PropertyPath(TranslateTransform.YProperty));

        sb.Children.Add(fade);
        sb.Children.Add(slide);
        sb.Begin();
    }

    // ---- donut chart ----

    private void AnimateDonutChart()
    {
        var breakdown = _viewModel.ClaimStatusBreakdown;
        var total = breakdown.Values.Sum();
        var approvedPercent = total > 0 && breakdown.TryGetValue(ClaimStatus.Approved, out var approved)
            ? (int)Math.Round(100.0 * approved / total)
            : 0;

        const int steps = 24;
        var current = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (s, e) =>
        {
            current++;
            var progress = current / (double)steps;
            DrawDonutChart(progress);
            CenterValueText.Text = $"{(int)Math.Round(approvedPercent * progress)}%";
            if (current >= steps) ((DispatcherTimer)s!).Stop();
        };
        timer.Start();
    }

    private void DrawDonutChart(double progress)
    {
        ChartCanvas.Children.Clear();

        var breakdown = _viewModel.ClaimStatusBreakdown;
        var total = breakdown.Values.Sum();
        if (total == 0) return;

        double startAngle = -90;
        var maxSweep = 360.0 * progress;
        double drawn = 0;

        foreach (var (status, count) in breakdown)
        {
            var fullSweep = 360.0 * count / total;
            var remaining = Math.Max(0, Math.Min(fullSweep, maxSweep - drawn));
            if (remaining <= 0.01) break;

            ChartCanvas.Children.Add(BuildArcSegment(90, 90, 78, startAngle, startAngle + remaining, _palette[status]));

            startAngle += fullSweep;
            drawn += remaining;
        }
    }

    private static Path BuildArcSegment(double cx, double cy, double radius, double startDeg, double endDeg, Color color)
    {
        Point ToPoint(double deg)
        {
            var rad = Math.PI * deg / 180.0;
            return new Point(cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad));
        }

        var start = ToPoint(startDeg);
        var end = ToPoint(endDeg);
        var isLargeArc = (endDeg - startDeg) > 180;

        var figure = new PathFigure { StartPoint = start, IsClosed = false };
        figure.Segments.Add(new ArcSegment
        {
            Point = end,
            Size = new Size(radius, radius),
            IsLargeArc = isLargeArc,
            SweepDirection = SweepDirection.Clockwise
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);

        return new Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 22,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
    }
}