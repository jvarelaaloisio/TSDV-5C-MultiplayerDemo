using System;
using System.Collections.Generic;

namespace Network
{
    public enum HandshakeResponseCodes : short
    {
        Undef = -1,
        Success = 0,
        DuplicatedNickname = 1,
    }
    public class NetHandshakeResponse : NetMessage<(HandshakeResponseCodes result, int clientId)>
    {
        private const int MessageSize = sizeof(HandshakeResponseCodes) + sizeof(int);

        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            if (message.Length < MessageSize)
            {
                return false;
            }
            Data = ((HandshakeResponseCodes)BitConverter.ToInt16(message),
                    BitConverter.ToInt32(message[sizeof(short)..]));
            return true;
        }

        public override MessageType GetMessageType()
            => MessageType.HandshakeResponse;

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(BitConverter.GetBytes((short)Data.result));
            currentData.AddRange(BitConverter.GetBytes(Data.clientId));

            return currentData.ToArray();
        }
    }
}