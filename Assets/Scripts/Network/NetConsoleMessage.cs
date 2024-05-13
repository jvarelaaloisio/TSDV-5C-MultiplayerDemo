using Network;

public class NetConsoleMessage : NetString
{
    public override MessageType GetMessageType() => MessageType.Console;
}