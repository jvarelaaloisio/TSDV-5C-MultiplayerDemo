using System.Collections.Generic;

namespace Network
{
    public class Client
    {
        public const int InvalidId = -1;
        public float timeStamp;
        private readonly MessageIdDictionary _messageIdDictionary;
        public int ID { get; set; } = InvalidId;
        public string Nickname { get; }
        public Dictionary<Model.Network.PacketType, ULongCounter> MessageCounters
        {
            get { return _messageIdDictionary.MessageCounters; }
        }

        public Client(int id,
                      float timeStamp,
                      string nickname)
        {
            this.timeStamp = timeStamp;
            _messageIdDictionary = new MessageIdDictionary();
            this.Nickname = nickname;
            this.ID = id;
        }
        public ulong GetMessageId(Model.Network.PacketType mt)
        {
            return _messageIdDictionary.GetMessageId(mt);
        }
        public ulong GetNextMessageId(Model.Network.PacketType mt)
        {
            return _messageIdDictionary.GetNextMessageId(mt);
        }

        public void SetMessageId(Model.Network.PacketType packetType, ulong messageCount)
        {
            _messageIdDictionary.SetMessageId(packetType, messageCount);
        }
    }
}