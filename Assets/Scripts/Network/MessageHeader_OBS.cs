using System;
using System.Collections.Generic;

namespace Network
{
    [Obsolete]
    public class MessageHeader_OBS
    {
        public const int Size = sizeof(MessageFlags_OBS) + sizeof(int) + sizeof(MessageType_OBS);
        public MessageFlags_OBS FlagsObs { get; set; }
        public int ClientId { get; set; }

        public MessageHeader_OBS(int clientId,
            MessageFlags_OBS flagsObs = MessageFlags_OBS.None)
        {
            ClientId = clientId;
            FlagsObs = flagsObs;
        }

        internal void Write(List<byte> outData, MessageType_OBS typeObs)
        {
            outData.AddRange(BitConverter.GetBytes((int)typeObs));
            outData.AddRange(BitConverter.GetBytes((int)FlagsObs));
            outData.AddRange(BitConverter.GetBytes((int)ClientId));
        }
    }
}