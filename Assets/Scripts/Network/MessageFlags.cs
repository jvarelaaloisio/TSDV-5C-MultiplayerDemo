﻿using System;

namespace Network
{
    [Flags]
    public enum MessageFlags
    {
        None = 0,
        IsSerialized = 1,
    }
}