using System;

namespace Network
{
    [Flags]
    [Obsolete]
    public enum MessageFlags_OBS
    {
        None = 0,
        IsSerialized,
        HasCheckSum,
        IsImportant,
    }
}