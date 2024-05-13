using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class NetMessage
    {
        private const int MessageHeaderSize = sizeof(int) * 2;
        public MessageFlags Flags { get; set; } = MessageFlags.None;

        public abstract MessageType GetMessageType();

        public byte[] GetBytes()
        {
            var outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes((int)Flags));
            return GetBytesInternal(outData);
        }
        
        protected abstract byte[] GetBytesInternal(List<byte> currentData);

        public bool TryDeserializeIntoSelf(byte[] message)
        {
            return message.Length >= MessageHeaderSize
                   && TryDeserializeIntoSelfInternal(message[MessageHeaderSize..]);
        }

        protected abstract bool TryDeserializeIntoSelfInternal(byte[] message);

        public static MessageFlags ReadFlags(byte[] data) => (MessageFlags)BitConverter.ToInt32(data, sizeof(int));
        public static MessageType ReadType(byte[] data) => (MessageType)BitConverter.ToInt32(data);
    }
    public abstract class NetMessage<T> : NetMessage
    {
        public T Data { get; set; }
    }
}