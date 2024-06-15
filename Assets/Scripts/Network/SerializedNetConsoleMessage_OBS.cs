using System;

namespace Network
{
    [Obsolete("Model_OBS.Network.Serialized.Impl.SerializedConsoleMessage")]
    public class SerializedNetConsoleMessage_OBS : SerializedNetString_OBS
    {
        public override MessageType_OBS GetMessageType() => MessageType_OBS.Console;
    }
}