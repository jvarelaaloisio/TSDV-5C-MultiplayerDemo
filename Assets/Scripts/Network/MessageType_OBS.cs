using System;

namespace Network
{
    [Obsolete]
    public enum MessageType_OBS
    {
        Invalid = -1,
        HandshakeRequest = 0,
        HandshakeResponse,
        ClientListUpdate,
        Console,
        Position,
    }
}