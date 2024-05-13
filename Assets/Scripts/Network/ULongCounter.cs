namespace Network
{
    public class ULongCounter
    {
        public ulong Value { get; set; } = 0L;
        public ulong GetNext => ++Value;
        public void Increase() => Value++;
    }
}