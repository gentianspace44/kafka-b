using System.Net.Sockets;

namespace VPS.Helpers
{
    public interface IGetStreamResults
    {
        Task<string> GetNetworkResult<T>(T requestObject, NetworkStream networkStream);
        Task<string> GetResults<T>(T requestObject, NetworkStream networkStream);
        string RemoveAllNamespaces(string xmlDocument);
    }
}
