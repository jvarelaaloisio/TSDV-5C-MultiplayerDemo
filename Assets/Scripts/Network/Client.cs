using System.Collections.Generic;
using System.Net;

namespace Network
{
    public struct Client
    {
        public const int InvalidId = -1;
        public float timeStamp;
        private readonly MessageIdDictionary _messageIdDictionary;
        public int ID { get; set; }
        public string Nickname { get; }
        public Dictionary<MessageType, ULongCounter> MessageCounters
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
        public ulong GetMessageId(MessageType mt)
        {
            return _messageIdDictionary.GetMessageId(mt);
        }
        public ulong GetNextMessageId(MessageType mt)
        {
            return _messageIdDictionary.GetNextMessageId(mt);
        }

        public void SetMessageId(MessageType messageType, ulong messageCount)
        {
            _messageIdDictionary.SetMessageId(messageType, messageCount);
        }
    }
}