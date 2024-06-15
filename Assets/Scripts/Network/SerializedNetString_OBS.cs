using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    [Obsolete("Model_OBS.Network.Serialized.Impl.SerializedStringMessage")]
    public abstract class SerializedNetString_OBS : SerializedNetMessage_OBS<string>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            if (!base.TryDeserializeIntoSelfInternal(message))
                return false;

            Data = Encoding.UTF8.GetString(message[sizeof(long)..]);
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(Encoding.UTF8.GetBytes(Data));
            return currentData.ToArray();
        }
    }
}