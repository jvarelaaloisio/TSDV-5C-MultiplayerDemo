using System.Collections.Generic;
using System.Text;

namespace Network
{
    public abstract class NetString : NetMessage<string>
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