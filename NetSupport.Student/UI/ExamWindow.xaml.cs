using System.Windows;
using System.Windows.Threading;
using NetSupport.Shared.Models;
using NetSupport.Student.Services;

namespace NetSupport.Student.UI;

public partial class ExamWindow : Window
{
    private readonly PushExamPayload _exam;
    private readonly TcpUpdateSender _updateSender;
    private int _currentIndex = 0;
    private Dictionary<int, string> _answers = new();
    private DispatcherTimer _timer;
    private TimeSpan _timeLeft;
    private bool _isSubmitted = false;

    public ExamWindow(PushExamPayload exam, TcpUpdateSender updateSender)
    {
        InitializeComponent();
        _exam = exam;
        _updateSender = updateSender;

        _timeLeft = TimeSpan.FromMinutes(_exam.DurationMinutes);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        LoadQuestion(_currentIndex);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
        TxtTimer.Text = _timeLeft.ToString(@"mm\:ss");

        if (_timeLeft.TotalSeconds <= 0)
        {
            ForceSubmit();
        }
    }

    private void LoadQuestion(int index)
    {
        var q = _exam.Questions[index];
        TxtProgress.Text = $"Question {index + 1} of {_exam.Questions.Count}";
        TxtQuestion.Text = q.QuestionText;
        RbOptionA.Content = q.OptionA;
        RbOptionB.Content = q.OptionB;
        RbOptionC.Content = q.OptionC;
        RbOptionD.Content = q.OptionD;

        // Temporarily unhook event so setting IsChecked doesn't trigger a network message
        RbOptionA.Checked -= Option_Checked;
        RbOptionB.Checked -= Option_Checked;
        RbOptionC.Checked -= Option_Checked;
        RbOptionD.Checked -= Option_Checked;

        RbOptionA.IsChecked = _answers.ContainsKey(index) && _answers[index] == "A";
        RbOptionB.IsChecked = _answers.ContainsKey(index) && _answers[index] == "B";
        RbOptionC.IsChecked = _answers.ContainsKey(index) && _answers[index] == "C";
        RbOptionD.IsChecked = _answers.ContainsKey(index) && _answers[index] == "D";

        RbOptionA.Checked += Option_Checked;
        RbOptionB.Checked += Option_Checked;
        RbOptionC.Checked += Option_Checked;
        RbOptionD.Checked += Option_Checked;

        BtnPrev.IsEnabled = index > 0;
        
        if (index == _exam.Questions.Count - 1)
        {
            BtnNext.Visibility = Visibility.Collapsed;
            BtnSubmit.Visibility = Visibility.Visible;
        }
        else
        {
            BtnNext.Visibility = Visibility.Visible;
            BtnSubmit.Visibility = Visibility.Collapsed;
        }
    }

    private async void Option_Checked(object sender, RoutedEventArgs e)
    {
        string selected = "A";
        if (RbOptionB.IsChecked == true) selected = "B";
        if (RbOptionC.IsChecked == true) selected = "C";
        if (RbOptionD.IsChecked == true) selected = "D";

        _answers[_currentIndex] = selected;

        // Send ANSWER_UPDATE
        int score = CalculateScore();
        var payload = new AnswerUpdatePayload
        {
            Ip = GetLocalIpAddress(),
            ScoreString = $"{score}/{_exam.Questions.Count}"
        };

        await _updateSender.SendUpdateAsync("ANSWER_UPDATE", payload);
    }

    private void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0) LoadQuestion(--_currentIndex);
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < _exam.Questions.Count - 1) LoadQuestion(++_currentIndex);
    }

    private void BtnSubmit_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to submit?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            ForceSubmit();
        }
    }

    public async void ForceSubmit()
    {
        if (_isSubmitted) return;
        _isSubmitted = true;
        _timer.Stop();

        int score = CalculateScore();
        var payload = new ExamResultPayload
        {
            Ip = GetLocalIpAddress(),
            FinalScore = $"{score}/{_exam.Questions.Count}"
        };

        await _updateSender.SendUpdateAsync("EXAM_RESULT", payload);

        MessageBox.Show($"Exam Finished! Your score: {score}/{_exam.Questions.Count}");
        this.Close();
    }

    private int CalculateScore()
    {
        int score = 0;
        foreach (var kvp in _answers)
        {
            if (_exam.Questions[kvp.Key].CorrectOption == kvp.Value)
            {
                score++;
            }
        }
        return score;
    }

    private string GetLocalIpAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        // Backdoor
        if (e.Key == System.Windows.Input.Key.Q && 
            System.Windows.Input.Keyboard.Modifiers == (System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift))
        {
            Application.Current.Shutdown();
            return;
        }

        if (e.Key == System.Windows.Input.Key.System && e.SystemKey == System.Windows.Input.Key.F4)
        {
            e.Handled = true; // Block Alt-F4
        }
        base.OnPreviewKeyDown(e);
    }
}
