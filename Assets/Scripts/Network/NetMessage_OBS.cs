using System;
using System.Collections.Generic;

namespace Network
{
    [Obsolete("Model_OBS.Network.Message")]
    public abstract class NetMessage_OBS
    {
        public abstract MessageType_OBS GetMessageType();
        
        public byte[] GetBytes(MessageHeader_OBS headerObs)
        {
            var outData = new List<byte>();
            headerObs.Write(outData, GetMessageType());
            return GetBytesInternal(outData);
        }

        protected abstract byte[] GetBytesInternal(List<byte> currentData);

        public bool TryDeserializeIntoSelf(byte[] message)
        {
            return message.Length >= MessageHeader_OBS.Size
                   && TryDeserializeIntoSelfInternal(message[MessageHeader_OBS.Size..]);
        }

        protected abstract bool TryDeserializeIntoSelfInternal(byte[] message);

        public static MessageType_OBS ReadType(byte[] data) => (MessageType_OBS)BitConverter.ToInt32(data);
        public static MessageFlags_OBS ReadFlags(byte[] data) => (MessageFlags_OBS)BitConverter.ToInt32(data, sizeof(int) * 1);
        public static int ReadClientId(byte[] data) => (int)BitConverter.ToInt32(data, sizeof(int) * 2);
    }
    
    /// <summary>
    /// A generic version of the <see cref="NetMessage_OBS"/> class, with a <see cref="Data"/> property.
    /// </summary>
    /// <typeparam name="T">The type for the <see cref="Data"/> property.</typeparam>
    [Obsolete("Model_OBS.Network.Message<T>")]
    public abstract class NetMessage_OBS<T> : NetMessage_OBS
    {
        /// <summary>
        /// Data contained by the message.
        /// </summary>
        public T Data { get; set; }
    }
}