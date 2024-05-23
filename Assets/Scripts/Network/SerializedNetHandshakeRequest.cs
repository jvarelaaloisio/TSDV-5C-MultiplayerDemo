namespace Network
{
    public class SerializedNetHandshakeRequest : NetString
    {
        public override MessageType GetMessageType() => MessageType.HandshakeRequest;
    }
}