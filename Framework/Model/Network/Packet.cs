using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Network
{
    /// <summary>
    /// A container that can be serialized from and rasterized into <see cref="byte"/>s.
    /// <para>This object contains a Header, and </para>
    /// </summary>
    public abstract class Packet
    {
        public byte[] GetBytes(Header header)
        {
            var outData = new List<byte>();
            header.Write(outData);
            return GetBytesInternal(outData);
        }

        protected abstract byte[] GetBytesInternal(List<byte> currentData);
        //TODO: Add method TryParse<T> where T : Packet
        public bool TryDeserializeIntoSelf(byte[] data)
        {
            return data.Length >= Header.Size
                   && TryDeserializeIntoSelfInternal(data[..Header.Size]);
        }

        protected abstract bool TryDeserializeIntoSelfInternal(byte[] data);

        [Obsolete("Use Header.TryParse")]
        public static PacketType ReadType(byte[] data) => (PacketType)BitConverter.ToInt32(data, 0);
        [Obsolete("Use Header.TryParse")]
        public static PacketFlags ReadFlags(byte[] data) => (PacketFlags)BitConverter.ToInt32(data, sizeof(int) * 1);
        [Obsolete("Use Header.TryParse")]
        public static int ReadClientId(byte[] data) => (int)BitConverter.ToInt32(data, sizeof(int) * 2);
    }
    
    /// <summary>
    /// A generic version of the <see cref="Packet"/> class, with a <see cref="Payload"/> property.
    /// </summary>
    /// <typeparam name="T">The type for the <see cref="Payload"/> property.</typeparam>
    public abstract class Packet<T> : Packet
    {
        /// <summary>
        /// Data contained by the message.
        /// </summary>
        public T Payload { get; set; }
    }
}