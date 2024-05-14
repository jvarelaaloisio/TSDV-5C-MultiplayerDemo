using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class NetMessage
    {
        protected readonly MessageHeader header = new();

        public abstract MessageType GetMessageType();
        public byte[] GetBytes()
        {
            header.Type = GetMessageType();
            var outData = new List<byte>();
            header.Write(outData);
            return GetBytesInternal(outData);
        }

        protected abstract byte[] GetBytesInternal(List<byte> currentData);

        public bool TryDeserializeIntoSelf(byte[] message)
        {
            return message.Length >= MessageHeader.Size
                   && TryDeserializeIntoSelfInternal(message[MessageHeader.Size..]);
        }

        protected abstract bool TryDeserializeIntoSelfInternal(byte[] message);

        public static MessageType ReadType(byte[] data) => (MessageType)BitConverter.ToInt32(data);
        public static MessageFlags ReadFlags(byte[] data) => (MessageFlags)BitConverter.ToInt32(data, sizeof(int) * 1);
        public static int ReadClientId(byte[] data) => (int)BitConverter.ToInt32(data, sizeof(int) * 2);
    }
    public abstract class NetMessage<T> : NetMessage
    {
        public T Data { get; set; }
    }
}