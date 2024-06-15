using System.Collections.Generic;
using System.Text;

namespace Model.Network.Impl
{
    [Packet(group:"native", uniqueId: 3)]
    public abstract class StringPacket : Packet<string>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            Payload = Encoding.UTF8.GetString(data);
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(Encoding.UTF8.GetBytes(Payload));
            return currentData.ToArray();
        }
    }
}