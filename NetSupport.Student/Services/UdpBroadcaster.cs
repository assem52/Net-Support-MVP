using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

/// <summary>
/// This class runs in the background and constantly shouts "I am here!" to the network.
/// </summary>
public class UdpBroadcaster
{
    private bool _isRunning = false;

    public async Task StartBroadcastingAsync(string studentName, string roomName)
    {
        _isRunning = true;
        
        // UdpClient is used to send UDP packets. We enable broadcast.
        using var udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        
        // We want to broadcast to the entire local network (255.255.255.255)
        var endPoint = new IPEndPoint(IPAddress.Broadcast, Constants.UdpBroadcastPort);

        while (_isRunning)
        {
            try
            {
                // 1. Create the payload with the student's info
                var payload = new StudentHelloPayload
                {
                    Name = studentName,
                    Ip = GetLocalIpAddress(),
                    Room = roomName
                };

                // 2. Wrap it in our standard message envelope
                var message = new NetworkMessage
                {
                    Type = "STUDENT_HELLO",
                    Payload = JsonSerializer.SerializeToElement(payload)
                };

                // 3. Convert to JSON string, then to bytes
                var json = JsonSerializer.Serialize(message);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                // 4. Send the shout!
                await udpClient.SendAsync(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                // In a real app we'd log this, but for MVP we just print to debug
                System.Diagnostics.Debug.WriteLine($"Broadcast error: {ex.Message}");
            }

            // Wait 5 seconds before shouting again
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    public void Stop()
    {
        _isRunning = false;
    }

    // A helper method to find the PC's actual IP address on the network
    private string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }
}
