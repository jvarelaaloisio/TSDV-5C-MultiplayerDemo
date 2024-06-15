﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Network.Impl
{
    [Packet(group:"native", uniqueId: 2)]
    public class ClientListUpdatePacket : Packet<(int clientId, string nickname)[]>
    {
        //TODO:Fix wrong de/serialization.
        protected override bool TryDeserializeIntoSelfInternal(byte[] data)
        {
            var clients = new List<(int clientId, string nickname)>();
            for (int i = 0; i < data.Length;)
            {
                var clientId = BitConverter.ToInt32(data, i);
                i += sizeof(int);
                var nickname = string.Empty;
                while (i < data.Length)
                {
                    var currentChar = (char)data[i];
                    //Gotta increase i before break :)
                    i++;
                    if (currentChar == '\n')
                        break;
                    nickname += currentChar;
                }
                clients.Add((clientId, nickname));
            }
            Payload = clients.ToArray();
            return true;
        }

        protected override byte[] GetBytesInternal(List<byte> currentData)
        {
            foreach (var (clientId, nickname) in Payload)
            {
                currentData.AddRange(BitConverter.GetBytes(clientId));
                currentData.AddRange(Encoding.UTF8.GetBytes(nickname));
                currentData.Add((byte)'\n');
            }
        
            return currentData.ToArray();
        }
    }
}