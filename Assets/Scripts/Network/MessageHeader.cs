using System;
using System.Collections.Generic;

namespace Network
{
    public class MessageHeader
    {
        public const int Size = sizeof(MessageFlags) + sizeof(int) + sizeof(MessageType);
        public MessageFlags Flags { get; set; }
        public int ClientId { get; set; }

        public MessageHeader(int clientId,
            MessageFlags flags = MessageFlags.None)
        {
            ClientId = clientId;
            Flags = flags;
        }

        internal void Write(List<byte> outData, MessageType type)
        {
            outData.AddRange(BitConverter.GetBytes((int)type));
            outData.AddRange(BitConverter.GetBytes((int)Flags));
            outData.AddRange(BitConverter.GetBytes((int)ClientId));
        }
    }
}