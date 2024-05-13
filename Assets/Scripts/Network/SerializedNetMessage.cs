using System;
using System.Collections.Generic;

namespace Network
{
    public abstract class SerializedNetMessage<T> : NetMessage<T>
    {
        public byte[] Serialized(ulong messageId)
        {
            var outData = new List<byte>();

            outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
            outData.AddRange(BitConverter.GetBytes(messageId));
            return GetBytesInternal(outData);
        }
        
        public byte[] Serialized(Func<MessageType, ulong> getMessageId)
        {
            var messageId = getMessageId(GetMessageType());
            return Serialized(messageId);
        }
    }
}