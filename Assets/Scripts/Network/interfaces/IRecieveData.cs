using System;
using System.Net;

namespace Network.interfaces
{
    [Obsolete]
    public interface IReceiveNetData_OBS
    {
        void OnReceiveData(byte[] data, IPEndPoint ipEndpoint);
    }
}