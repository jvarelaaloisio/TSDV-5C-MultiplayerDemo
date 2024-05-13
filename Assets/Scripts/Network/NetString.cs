using System.Collections.Generic;
using System.Text;

namespace Network
{
    public abstract class NetString : SerializedNetMessage<string>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            int charCount = (message.Length) / sizeof(byte);
            var outData = new char[charCount];
            for (int i = 0; i < charCount; i++)
            {
                var c = (char)
                    message[i * sizeof(byte)];
                outData[i] = c;
            }

            Data = new string(outData);
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            currentData.AddRange(Encoding.UTF8.GetBytes(Data));
            return currentData.ToArray();
        }
    }
}