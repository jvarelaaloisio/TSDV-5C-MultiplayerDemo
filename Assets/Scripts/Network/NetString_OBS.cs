using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    [Obsolete("Model_OBS.Network.Impl.StringMessage")]
    public abstract class NetString_OBS : NetMessage_OBS<string>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            Data = Encoding.UTF8.GetString(message);
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(Encoding.UTF8.GetBytes(Data));
            return currentData.ToArray();
        }
    }
}