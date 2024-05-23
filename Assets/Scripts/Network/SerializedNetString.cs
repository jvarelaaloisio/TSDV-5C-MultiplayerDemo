using System.Collections.Generic;
using System.Text;

namespace Network
{
    public abstract class SerializedNetString : SerializedNetMessage<string>
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