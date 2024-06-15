using System.Collections.Generic;

namespace Network
{
    public class MessageIdDictionary
    {
        public Dictionary<Model.Network.PacketType, ULongCounter> MessageCounters { get; } = new();

        public ulong GetMessageId(Model.Network.PacketType mt)
        {
            return MessageCounters.TryGetValue(mt, out var counter)
                       ? counter.Value
                       : 0L;
        }

        public ulong GetNextMessageId(Model.Network.PacketType packetTypeObs)
        {
            if (MessageCounters.TryGetValue(packetTypeObs, out var counter))
                return counter.GetNext;
        
            MessageCounters.Add(packetTypeObs, new ULongCounter());
            counter = MessageCounters[packetTypeObs];
            return counter.GetNext;
        }

        public void SetMessageId(Model.Network.PacketType packetTypeObs, ulong messageCount)
        {
            if (MessageCounters.TryGetValue(packetTypeObs, out var counter))
                counter.Value = messageCount;
            else
                MessageCounters.Add(packetTypeObs, new ULongCounter {Value = messageCount});
        }
    }
}