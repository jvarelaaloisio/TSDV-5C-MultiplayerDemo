using System;
using System.Collections.Generic;
using Model.Network;
using UnityEngine;

namespace Network
{
    //ASK: Should I keep this in unity only?
    public class NetVector3 : Packet<Vector3>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            if (data.Length < sizeof(float) * 3)
            {
                return false;
            }
            Payload = new Vector3(
                               BitConverter.ToSingle(data),
                               BitConverter.ToSingle(data, sizeof(float)),
                               BitConverter.ToSingle(data, sizeof(float))
                              );
            return true;
        }

        //TODO: Kill enum
        public override Model.Network.PacketType GetMessageType()
        {
            return Model.Network.PacketType.Position;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(BitConverter.GetBytes(Payload.x));
            currentData.AddRange(BitConverter.GetBytes(Payload.y));
            currentData.AddRange(BitConverter.GetBytes(Payload.z));

            return currentData.ToArray();
        }
    }
}