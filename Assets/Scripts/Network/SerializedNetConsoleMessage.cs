namespace Network
{
    public class SerializedNetConsoleMessage : SerializedNetString
    {
        public override MessageType GetMessageType() => MessageType.Console;
    }
}