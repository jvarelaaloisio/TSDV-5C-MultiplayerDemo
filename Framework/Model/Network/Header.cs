using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Network
{
    public class Header
    {
        private const int SizeOfClientId = sizeof(int);
        private const int SizeOfTypeId = sizeof(int);
        private const int SizeOfTypeGroup = sizeof(char) * 4;
        public const int Size = sizeof(PacketFlags) + SizeOfClientId + SizeOfTypeId;
        
        public string TypeGroup { get; set; }
        public int TypeId { get; set; }
        public PacketFlags Flags { get; set; }
        public int ClientId { get; set; }

        public Header(string typeGroup,
                      int typeId,
                      int clientId,
                      PacketFlags flags = PacketFlags.None)
        {
            ClientId = clientId;
            TypeGroup = typeGroup;
            TypeId = typeId;
            Flags = flags;
        }

        public void Write(List<byte> outData)
        {
            outData.AddRange(Encoding.UTF8.GetBytes(TypeGroup));
            outData.AddRange(BitConverter.GetBytes((int)TypeId));
            outData.AddRange(BitConverter.GetBytes((int)Flags));
            outData.AddRange(BitConverter.GetBytes((int)ClientId));
        }

        public static bool TryParse(byte[] data, out Header header)
        {
            if (data.Length < Size)
            {
                header = default;
                return false;
            }
            var typeGroup = Encoding.UTF8.GetString(data, 0, SizeOfTypeGroup);
            var typeId = BitConverter.ToInt32(data, SizeOfTypeGroup);
            var flags = (PacketFlags)BitConverter.ToInt32(data, SizeOfTypeGroup + SizeOfTypeId);
            var clientId = (int)BitConverter.ToInt32(data, SizeOfTypeGroup + SizeOfClientId + sizeof(PacketFlags));
            header = new Header(typeGroup, typeId, clientId, flags);
            return true;
        }
    }
}