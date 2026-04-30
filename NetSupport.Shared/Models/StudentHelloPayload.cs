using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// The payload sent by the student app to announce itself to the tutor.
/// </summary>
public class StudentHelloPayload : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _ip = string.Empty;
    private string _room = string.Empty;
    private bool _isLocked = false;
    private bool _isReady = false;

    /// <summary>
    /// The name of the student's PC (e.g., "PC-01").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    /// <summary>
    /// The IP address of the student's PC.
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip { get => _ip; set { _ip = value; OnPropertyChanged(); } }

    /// <summary>
    /// The room the student is in, used by the tutor to filter.
    /// </summary>
    [JsonPropertyName("room")]
    public string Room { get => _room; set { _room = value; OnPropertyChanged(); } }

    /// <summary>
    /// UI Helper flag to show if the student is currently locked.
    /// It is ignored by JSON since it is just used locally in the Tutor app.
    /// </summary>
    [JsonIgnore]
    public bool IsLocked { get => _isLocked; set { _isLocked = value; OnPropertyChanged(); } }

    /// <summary>
    /// UI Helper flag to show if the student has logged into the exam and is ready.
    /// </summary>
    [JsonIgnore]
    public bool IsReady { get => _isReady; set { _isReady = value; OnPropertyChanged(); } }

    private string _score = string.Empty;
    /// <summary>
    /// UI Helper to show live score updates (e.g., "3/5").
    /// </summary>
    [JsonIgnore]
    public string Score { get => _score; set { _score = value; OnPropertyChanged(); } }

    /// <summary>
    /// Holds the final detailed analytics of the student's exam for PDF generation.
    /// </summary>
    [JsonIgnore]
    public System.Collections.Generic.List<StudentAnswerInfo>? DetailedResults { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
