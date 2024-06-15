using System;
using System.Collections.Generic;

namespace Network
{
    [Obsolete("Model_OBS.Network.Serialized.SerializedMessage")]
    public abstract class SerializedNetMessage_OBS : NetMessage_OBS
    {
        public ulong MessageId { get; private set; }

        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            if (message.Length < sizeof(long))
                return false;
            MessageId = BitConverter.ToUInt64(message);
            return true;
        }

        public byte[] Serialized(ulong messageId, MessageHeader_OBS headerObs)
        {
            var outData = new List<byte>();

            headerObs.Write(outData, GetMessageType());
            outData.AddRange(BitConverter.GetBytes(messageId));
            return GetBytesInternal(outData);
        }
        
        public byte[] Serialized(Func<MessageType_OBS, ulong> getMessageId, MessageHeader_OBS headerObs)
        {
            var messageId = getMessageId(GetMessageType());
            return Serialized(messageId, headerObs);
        }

        public static ulong ReadMessageId(byte[] data)
        {
            return BitConverter.ToUInt64(data, MessageHeader_OBS.Size);
        }
    }
    [Obsolete("Model_OBS.Network.Serialized.SerializedMessage")]
    public abstract class SerializedNetMessage_OBS<T> : SerializedNetMessage_OBS
    {
        public T Data { get; set; }
    }
}