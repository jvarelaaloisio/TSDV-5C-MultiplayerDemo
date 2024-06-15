using System;
using System.Collections.Generic;

namespace Network
{
    [Obsolete("Model_OBS.Network.Impl.HandshakeResponseCodes")]
    public enum HandshakeResponseCodes : short
    {
        Undef = -1,
        Success = 0,
        DuplicatedNickname = 1,
    }
    [Obsolete("Model_OBS.Network.Impl.HandshakeResponse")]
    public class NetHandshakeResponse_OBS : NetMessage_OBS<(HandshakeResponseCodes result, int clientId)>
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

        public override MessageType_OBS GetMessageType()
            => MessageType_OBS.HandshakeResponse;

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(BitConverter.GetBytes((short)Data.result));
            currentData.AddRange(BitConverter.GetBytes(Data.clientId));

            return currentData.ToArray();
        }
    }
}