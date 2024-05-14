namespace Network
{
    public enum MessageType
    {
        Invalid = -1,
        HandshakeRequest = 0,
        HandshakeResponse,
        ClientListUpdate,
        Console,
        Position,
    }
}