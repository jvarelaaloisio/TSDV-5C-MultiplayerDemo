using System;
using System.Collections.Generic;
using System.Text;

public class NetConsoleMessage : IMessage<string>
{
    private const int MessageHeaderSize = 12;
    private static ulong lastMsgID = 0;
    public string data;

    public NetConsoleMessage(string data)
    {
        this.data = data;
    }

    public string Deserialize(byte[] message)
    {
        int charCount = (message.Length - MessageHeaderSize) / sizeof(byte);
        var outData = new char[charCount];
        for (int i = 0; i < charCount; i++)
        {
            var c = (char)
                message[i * sizeof(byte) + MessageHeaderSize];
            outData[i] = c;
        }

        return new string(outData);
    }

    public MessageType GetMessageType()
    {
        return MessageType.Console;
    }

    public byte[] Serialize()
    {
        List<byte> outData = new List<byte>();

        outData.AddRange(BitConverter.GetBytes((int)GetMessageType()));
        outData.AddRange(BitConverter.GetBytes(lastMsgID++));
        outData.AddRange(Encoding.UTF8.GetBytes(data));
        return outData.ToArray();
    }

    //Dictionary<Client,Dictionary<msgType,int>>
}