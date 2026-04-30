using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

/// <summary>
/// Helper class to connect to a student via TCP and send commands.
/// </summary>
public class TcpCommandSender
{
    public async Task SendCommandAsync(string ipAddress, string commandType, object? customPayload = null)
    {
        try
        {
            // Connect directly to the specific student
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, Constants.TcpCommandPort);

            // Prepare the JSON message
            var message = new NetworkMessage
            {
                Type = commandType,
                // Serialize custom payload, or default to an empty object for Lock/Unlock
                Payload = JsonSerializer.SerializeToElement(customPayload ?? new { })
            };

            var json = JsonSerializer.Serialize(message);
            
            // Send it over the stream
            using var stream = tcpClient.GetStream();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);
            
            // Stream and Client close automatically because of 'using' statements
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send TCP command: {ex.Message}");
            // We might want to show an error to the user here in a real app
        }
    }
}
