using System;
using System.Collections.Generic;

namespace Network
{
    public class MessageHeader
    {
        public const int Size = sizeof(MessageFlags) + sizeof(int) + sizeof(MessageType);
        public MessageFlags Flags { get; set; } = MessageFlags.None;
        public int ClientId { get; set; } = Client.InvalidId;
        public MessageType Type { get; set; } = MessageType.Invalid;

        internal void Write(List<byte> outData)
        {
            outData.AddRange(BitConverter.GetBytes((int)Type));
            outData.AddRange(BitConverter.GetBytes((int)Flags));
            outData.AddRange(BitConverter.GetBytes((int)ClientId));
        }
    }
}