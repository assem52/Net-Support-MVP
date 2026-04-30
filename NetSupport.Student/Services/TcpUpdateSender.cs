using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using NetSupport.Shared;
using NetSupport.Shared.Models;

namespace NetSupport.Student.Services;

public class TcpUpdateSender
{
    private readonly string _tutorIp;

    public TcpUpdateSender(string tutorIp)
    {
        _tutorIp = tutorIp;
    }

    public async Task SendUpdateAsync(string type, object payload)
    {
        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(_tutorIp, Constants.TcpUpdatePort);

            var message = new NetworkMessage
            {
                Type = type,
                Payload = JsonSerializer.SerializeToElement(payload)
            };

            var json = JsonSerializer.Serialize(message);
            
            using var stream = tcpClient.GetStream();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send update: {ex.Message}");
        }
    }
}
