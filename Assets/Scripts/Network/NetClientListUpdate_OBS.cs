using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    [Obsolete("Model_OBS.Network.Impl.ClientListUpdate")]
    public class NetClientListUpdate_OBS : NetMessage_OBS<(int clientId, string nickname)[]>
    {
        //TODO:Fix wrong de/serialization.
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            var clients = new List<(int clientId, string nickname)>();
            for (int i = 0; i < message.Length;)
            {
                var clientId = BitConverter.ToInt32(message, i);
                i += sizeof(int);
                var nickname = string.Empty;
                while (i < message.Length)
                {
                    var currentChar = (char)message[i];
                    //Gotta increase i before break :)
                    i++;
                    if (currentChar == '\n')
                        break;
                    nickname += currentChar;
                }
                clients.Add((clientId, nickname));
            }
            Data = clients.ToArray();
            return true;
        }
        
        public override MessageType_OBS GetMessageType()
            => MessageType_OBS.ClientListUpdate;

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            foreach (var (clientId, nickname) in Data)
            {
                currentData.AddRange(BitConverter.GetBytes(clientId));
                currentData.AddRange(Encoding.UTF8.GetBytes(nickname));
                currentData.Add((byte)'\n');
            }
        
            return currentData.ToArray();
        }
    }
}