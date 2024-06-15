using Model.Network;
using Model.Network.Impl;
using Model.Network.Serialized.Impl;

namespace MatchMakerModel
{
    [Packet(group: "mMaker", uniqueId:000)]
    public class MatchMakerHandshakeRequestPacket : SerializedHandshakeRequestPacket
    {
    }
    [Packet(group: "mMaker", uniqueId:001)]
    public class MatchMakerHandshakeResponsePacket : HandshakeResponsePacket
    {
    }
}