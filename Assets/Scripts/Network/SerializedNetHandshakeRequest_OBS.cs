using System;

namespace Network
{
    [Obsolete("Model_OBS.Network.Serialized.Impl.SerializedHandshakeRequestMessage")]
    public class SerializedNetHandshakeRequest_OBS : NetString_OBS
    {
        public override MessageType_OBS GetMessageType() => MessageType_OBS.HandshakeRequest;
    }
}