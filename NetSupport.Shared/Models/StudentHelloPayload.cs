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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
