using System.Collections.Generic;

namespace Network
{
    public class MessageIdDictionary
    {
        public Dictionary<MessageType, ULongCounter> MessageCounters { get; } = new();

        public ulong GetMessageId(MessageType mt)
        {
            return MessageCounters.TryGetValue(mt, out var counter)
                       ? counter.Value
                       : 0L;
        }

        public ulong GetNextMessageId(MessageType messageType)
        {
            if (MessageCounters.TryGetValue(messageType, out var counter))
                return counter.GetNext;
        
            MessageCounters.Add(messageType, new ULongCounter());
            counter = MessageCounters[messageType];
            return counter.GetNext;
        }

        public void SetMessageId(MessageType messageType, ulong messageCount)
        {

            if (MessageCounters.TryGetValue(messageType, out var counter))
                counter.Value = messageCount;
            else
                MessageCounters.Add(messageType, new ULongCounter {Value = messageCount});
        }
    }
}