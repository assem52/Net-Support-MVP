namespace NetSupport.Shared;

/// <summary>
/// Contains all the hardcoded settings and port numbers used across the applications.
/// </summary>
public static class Constants
{
    // The port used by Student app to broadcast its presence, and Tutor app to listen.
    public const int UdpBroadcastPort = 9000;
    
    // The port used by the Tutor to send commands (like LOCK/UNLOCK) to the Student.
    public const int TcpCommandPort = 9001;
    
    // The port used by the Student to send updates (like Answers) to the Tutor.
    public const int TcpUpdatePort = 9002;
}
