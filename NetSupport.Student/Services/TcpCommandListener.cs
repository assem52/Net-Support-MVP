using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

public class CommandReceivedEventArgs : EventArgs
{
    public string TutorIp { get; set; } = string.Empty;
    public NetworkMessage Message { get; set; } = new();
}

/// <summary>
/// Listens for direct TCP commands (like LOCK/UNLOCK/PUSH_EXAM) from the Tutor.
/// Binds to port 0 so the OS assigns a free random port, enabling multiple
/// Student instances to run simultaneously on the same machine.
/// </summary>
public class TcpCommandListener
{
    private bool _isRunning = false;
    private TcpListener? _tcpListener;

    /// <summary>
    /// The actual port the OS assigned after binding to port 0.
    /// This must be broadcast to the Tutor so it knows where to connect.
    /// </summary>
    public int AssignedPort { get; private set; } = 0;

    public event EventHandler<CommandReceivedEventArgs>? CommandReceived;

    public void StartListening()
    {
        _isRunning = true;
        // Bind to port 0: the OS will automatically assign a free port
        _tcpListener = new TcpListener(IPAddress.Any, 0);
        _tcpListener.Start();

        // Read back which port was actually assigned
        AssignedPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;

        Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (_isRunning)
        {
            try
            {
                using var client = await _tcpListener!.AcceptTcpClientAsync();
                
                // Extract the Tutor's IP address so we know who to send answers back to
                var remoteIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();
                
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                
                var json = await reader.ReadToEndAsync();
                
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                    if (message != null)
                    {
                        CommandReceived?.Invoke(this, new CommandReceivedEventArgs 
                        { 
                            TutorIp = remoteIp, 
                            Message = message 
                        });
                    }
                }
            }
            catch (ObjectDisposedException) { }
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
