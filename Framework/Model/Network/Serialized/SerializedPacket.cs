using System;
using System.Collections.Generic;

namespace Model.Network.Serialized
{
    public abstract class SerializedPacket : Packet
    {
        protected const int SizeOfId = sizeof(long);
        public ulong MessageId { get; private set; }

        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            if (data.Length < SizeOfId)
                return false;
            MessageId = BitConverter.ToUInt64(data, 0);
            return true;
        }

        public byte[] Serialized(ulong messageId, Header header)
        {
            var outData = new List<byte>();

            header.Write(outData);
            outData.AddRange(BitConverter.GetBytes(messageId));
            return GetBytesInternal(outData);
        }
        
        public byte[] Serialized(Func<Header, ulong> getMessageId, Header header)
        {
            var messageId = getMessageId(header);
            return Serialized(messageId, header);
        }

        public static ulong ReadMessageId(byte[] data)
        {
            return BitConverter.ToUInt64(data, Header.Size);
        }
    }
    public abstract class SerializedPacket<T> : SerializedPacket
    {
        public T Data { get; set; }
    }
}