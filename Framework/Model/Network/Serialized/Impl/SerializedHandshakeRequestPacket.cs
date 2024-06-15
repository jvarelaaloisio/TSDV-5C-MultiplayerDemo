using Model.Network.Impl;

namespace Model.Network.Serialized.Impl
{
    [Packet(group:"native", uniqueId: 0)]
    public class SerializedHandshakeRequestPacket : StringPacket { }
}