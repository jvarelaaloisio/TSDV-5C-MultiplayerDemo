using System.Net;

public interface IReceiveNetData
{
    void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
}