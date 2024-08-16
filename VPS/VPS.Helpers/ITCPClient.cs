using System.Net.Sockets;

namespace VPS.Helpers
{
    public interface ITcpClient
    {
        Task ConnectAsync(string ip, int port);

        NetworkStream GetStream();
    }
}
