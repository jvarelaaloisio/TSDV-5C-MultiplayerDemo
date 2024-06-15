using System;
using System.Collections.Generic;

namespace Model.Network.Impl
{
    public enum HandshakeResponseCodes : short
    {
        Undef = -1,
        Success = 0,
        DuplicatedNickname = 1,
    }
    
    [Packet(group:"native", uniqueId: 1)]
    public class HandshakeResponsePacket : Packet<(HandshakeResponseCodes result, int clientId)>
    {
        private const int MessageSize = sizeof(HandshakeResponseCodes) + sizeof(int);

        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            if (data.Length < MessageSize)
            {
                return false;
            }
            Payload = ((HandshakeResponseCodes)BitConverter.ToInt16(data, 0),
                    BitConverter.ToInt32(data, sizeof(short)));
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(BitConverter.GetBytes((short)Payload.result));
            currentData.AddRange(BitConverter.GetBytes(Payload.clientId));

            return currentData.ToArray();
        }
    }
}