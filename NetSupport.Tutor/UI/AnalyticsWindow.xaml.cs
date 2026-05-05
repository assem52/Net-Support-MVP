using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.UI;

public partial class AnalyticsWindow : Window
{
    private readonly ObservableCollection<StudentHelloPayload> _students;
    private readonly DispatcherTimer _refreshTimer;

    public AnalyticsWindow(ObservableCollection<StudentHelloPayload> students)
    {
        InitializeComponent();
        _students = students;

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _refreshTimer.Tick += (s, e) => UpdateDashboard();
        _refreshTimer.Start();

        UpdateDashboard();
    }

    private void UpdateDashboard()
    {
        var studentList = _students.ToList();
        var finishedStudents = studentList.Where(s => !string.IsNullOrEmpty(s.Score)).ToList();

        if (!finishedStudents.Any()) return;

        // 1. Summary Stats
        var scores = finishedStudents.Select(s => ParseScore(s.Score)).ToList();
        float avg = scores.Average();
        float max = scores.Max();
        int passed = scores.Count(s => s >= 50);

        TxtAvg.Text = $"{avg:F0}%";
        TxtSubmissions.Text = finishedStudents.Count.ToString();
        TxtPassRate.Text = $"{(float)passed / finishedStudents.Count * 100:F0}%";
        TxtMax.Text = $"{max:F0}%";

        // 2. Score Distribution
        var distribution = new List<ChartBar>
        {
            new ChartBar { Label = "76-100%", Value = scores.Count(s => s > 75), Color = "#10B981", MaxValue = finishedStudents.Count },
            new ChartBar { Label = "51-75%", Value = scores.Count(s => s > 50 && s <= 75), Color = "#3B82F6", MaxValue = finishedStudents.Count },
            new ChartBar { Label = "26-50%", Value = scores.Count(s => s > 25 && s <= 50), Color = "#F59E0B", MaxValue = finishedStudents.Count },
            new ChartBar { Label = "0-25%", Value = scores.Count(s => s <= 25), Color = "#EF4444", MaxValue = finishedStudents.Count }
        };
        ScoreDistributionChart.ItemsSource = distribution;

        // 3. Question Success Rates
        var allAnswers = finishedStudents.Where(s => s.DetailedResults != null).SelectMany(s => s.DetailedResults!).ToList();
        if (allAnswers.Any())
        {
            var questionStats = allAnswers.GroupBy(a => a.QuestionText)
                .Select(g => new QuestionStat
                {
                    Text = g.Key,
                    Rate = (float)g.Count(a => a.IsCorrect) / g.Count() * 100
                })
                .OrderBy(q => q.Rate)
                .ToList();

            QuestionChart.ItemsSource = questionStats;

            if (questionStats.Any())
            {
                var hardest = questionStats.First();
                if (hardest.Rate < 50)
                {
                    HardestAlert.Visibility = Visibility.Visible;
                    TxtHardestQ.Text = $"{hardest.Text} ({hardest.Rate:F0}% success)";
                }
                else
                {
                    HardestAlert.Visibility = Visibility.Collapsed;
                }
            }
        }
    }

    private float ParseScore(string score)
    {
        if (string.IsNullOrEmpty(score)) return 0;
        var cleanScore = score.Replace("FINAL: ", "").Trim();
        var parts = cleanScore.Split('/');
        if (parts.Length == 2 && float.TryParse(parts[0], out float correct) && float.TryParse(parts[1], out float total) && total > 0)
        {
            return (correct / total) * 100;
        }
        return 0;
    }
}

public class ChartBar
{
    public string Label { get; set; } = "";
    public int Value { get; set; }
    public int MaxValue { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public double BarWidth => MaxValue > 0 ? (double)Value / MaxValue * 200 : 0; // Scaled to 200px container
}

public class QuestionStat
{
    public string Text { get; set; } = "";
    public float Rate { get; set; }
    public string RateString => $"{Rate:F0}%";
    public string Color => Rate < 50 ? "#EF4444" : (Rate < 80 ? "#F59E0B" : "#10B981");
    public double BarWidth => Rate * 5; // Scaled to 500px container roughly
}
