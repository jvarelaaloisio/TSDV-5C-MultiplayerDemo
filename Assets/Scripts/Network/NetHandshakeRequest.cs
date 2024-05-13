namespace Network
{
    public class NetHandshakeRequest : NetString
    {
        public override MessageType GetMessageType() => MessageType.HandshakeRequest;
    }
}