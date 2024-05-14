using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class SerializedNetMessage : NetMessage
    {
        public byte[] Serialized(ulong messageId)
        {
            var outData = new List<byte>();

            header.Write(outData);
            outData.AddRange(BitConverter.GetBytes(messageId));
            return GetBytesInternal(outData);
        }
        
        public byte[] Serialized(Func<MessageType, ulong> getMessageId)
        {
            var messageId = getMessageId(GetMessageType());
            return Serialized(messageId);
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