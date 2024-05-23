using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class SerializedNetMessage : NetMessage
    {
        public ulong MessageId { get; private set; }

        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            if (message.Length < sizeof(long))
                return false;
            MessageId = BitConverter.ToUInt64(message);
            return true;
        }

        public byte[] Serialized(ulong messageId, MessageHeader header)
        {
            var outData = new List<byte>();

            header.Write(outData, GetMessageType());
            outData.AddRange(BitConverter.GetBytes(messageId));
            return GetBytesInternal(outData);
        }
        
        public byte[] Serialized(Func<MessageType, ulong> getMessageId, MessageHeader header)
        {
            var messageId = getMessageId(GetMessageType());
            return Serialized(messageId, header);
        }

        public static ulong ReadMessageId(byte[] data)
        {
            return BitConverter.ToUInt64(data, MessageHeader.Size);
        }
    }
    public abstract class SerializedNetMessage<T> : SerializedNetMessage
    {
        public T Data { get; set; }
    }
}