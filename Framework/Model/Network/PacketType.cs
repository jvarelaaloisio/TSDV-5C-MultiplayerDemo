using System;

namespace Model.Network
{
    [Obsolete]
    public enum PacketType
    {
        Invalid = -1,
        HandshakeRequest = 0,
        HandshakeResponse,
        ClientListUpdate,
        Console,
        Position,
    }
}