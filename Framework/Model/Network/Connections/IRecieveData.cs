using System.Net;

namespace Model.Network.Connections
{
    public interface IReceiveNetData
    {
        void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
    }
}