using System.Collections.Generic;
using System.Text;

namespace Model.Network.Serialized.Impl
{
    [Packet(group:"native", uniqueId: 4)]
    public abstract class SerializedStringPacket : SerializedPacket<string>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            if (!base.TryDeserializeIntoSelfInternal(data))
                return false;

            Data = Encoding.UTF8.GetString(data, SizeOfId, data.Length - SizeOfId);
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(Encoding.UTF8.GetBytes(Data));
            return currentData.ToArray();
        }
    }
}