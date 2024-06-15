using System;

namespace Model.Network
{
    [Flags]
    public enum PacketFlags
    {
        None = 0,
        IsSerialized = 1,
        HasCheckSum = 2,
        IsImportant = 4,
    }
}