using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

/// <summary>
/// Listens on Port 9002 for updates coming back from the Student (like "I am ready").
/// </summary>
public class TcpUpdateListener
{
    private bool _isRunning = false;
    private TcpListener? _tcpListener;

    public event EventHandler<StudentReadyPayload>? StudentReadyReceived;

    public void StartListening()
    {
        _isRunning = true;
        _tcpListener = new TcpListener(IPAddress.Any, Constants.TcpUpdatePort);
        _tcpListener.Start();

        Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (_isRunning)
        {
            try
            {
                using var client = await _tcpListener!.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                
                var json = await reader.ReadToEndAsync();
                
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                    if (message != null && message.Type == "STUDENT_READY")
                    {
                        var payload = JsonSerializer.Deserialize<StudentReadyPayload>(message.Payload.GetRawText());
                        if (payload != null)
                        {
                            StudentReadyReceived?.Invoke(this, payload);
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { /* Normal during shutdown */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Listener error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
    }
}
