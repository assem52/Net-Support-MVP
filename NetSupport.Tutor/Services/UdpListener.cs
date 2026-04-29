using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

/// <summary>
/// This class listens for shouts from students on the network.
/// </summary>
public class UdpListener
{
    private bool _isRunning = false;
    private UdpClient? _udpClient;

    // We use an event to notify the UI when a student is discovered
    public event EventHandler<StudentHelloPayload>? StudentDiscovered;

    public void StartListening()
    {
        _isRunning = true;
        
        // We bind to the broadcast port to listen
        _udpClient = new UdpClient(Constants.UdpBroadcastPort);

        // Run the listener loop on a background thread so it doesn't freeze the UI
        Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (_isRunning)
        {
            try
            {
                // Wait for any UDP packet
                var result = await _udpClient!.ReceiveAsync();
                var json = System.Text.Encoding.UTF8.GetString(result.Buffer);

                // Try to parse the JSON envelope
                var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                
                if (message != null && message.Type == "STUDENT_HELLO")
                {
                    // If it's a student hello, extract the payload
                    var payload = JsonSerializer.Deserialize<StudentHelloPayload>(message.Payload.GetRawText());
                    if (payload != null)
                    {
                        // Raise the event to notify the UI
                        StudentDiscovered?.Invoke(this, payload);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Normal when shutting down
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Listener error: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _udpClient?.Close();
    }
}
