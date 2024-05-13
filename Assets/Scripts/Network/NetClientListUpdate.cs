using System;
using System.Collections.Generic;
using System.Text;

namespace Network
{
    public class NetClientListUpdate : NetMessage<(int clientId, string nickname)[]>
    {
        protected override bool TryDeserializeIntoSelfInternal(byte[] message)
        {
            var clients = new List<(int clientId, string nickname)>();
            for (int i = 0; i < message.Length;)
            {
                var clientId = BitConverter.ToInt32(message, i);
                i += sizeof(int);
                var nickName = string.Empty;
                while (i < message.Length)
                {
                    var currentChar = (char)message[i];
                    if (currentChar == '\n')
                        break;
                    nickName += currentChar;
                    i++;
                }
                clients.Add((clientId, nickName));
            }
            Data = clients.ToArray();
            return true;
        }
        public override MessageType GetMessageType()
            => MessageType.ClientListUpdate;

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            foreach (var (clientId, nickname) in Data)
            {
                currentData.AddRange(BitConverter.GetBytes(clientId));
                currentData.AddRange(Encoding.UTF8.GetBytes(nickname));
            }
        
            return currentData.ToArray();
        }
    }
}