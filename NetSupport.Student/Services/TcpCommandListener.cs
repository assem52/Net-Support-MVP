using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

/// <summary>
/// Listens for direct TCP commands (like LOCK/UNLOCK) from the Tutor.
/// </summary>
public class TcpCommandListener
{
    private bool _isRunning = false;
    private TcpListener? _tcpListener;

    public event EventHandler? LockCommandReceived;
    public event EventHandler? UnlockCommandReceived;

    public void StartListening()
    {
        _isRunning = true;
        // Listen on all network interfaces
        _tcpListener = new TcpListener(IPAddress.Any, Constants.TcpCommandPort);
        _tcpListener.Start();

        Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (_isRunning)
        {
            try
            {
                // AcceptTcpClientAsync pauses here until a Tutor connects
                using var client = await _tcpListener!.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                
                // Read the JSON message from the stream
                var json = await reader.ReadToEndAsync();
                
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                    if (message != null)
                    {
                        if (message.Type == "LOCK")
                        {
                            LockCommandReceived?.Invoke(this, EventArgs.Empty);
                        }
                        else if (message.Type == "UNLOCK")
                        {
                            UnlockCommandReceived?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { /* Normal when shutting down */ }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP Listener error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _tcpListener?.Stop();
    }
}
