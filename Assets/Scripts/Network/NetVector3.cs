using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class NetVector3 : NetMessage<Vector3>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            if (message.Length < sizeof(float) * 3)
            {
                return false;
            }
            Data = new Vector3(
                               BitConverter.ToSingle(message),
                               BitConverter.ToSingle(message, sizeof(float)),
                               BitConverter.ToSingle(message, sizeof(float))
                              );
            return true;
        }

        public override MessageType GetMessageType()
        {
            return MessageType.Position;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(BitConverter.GetBytes(Data.x));
            currentData.AddRange(BitConverter.GetBytes(Data.y));
            currentData.AddRange(BitConverter.GetBytes(Data.z));

            return currentData.ToArray();
        }
    }
}