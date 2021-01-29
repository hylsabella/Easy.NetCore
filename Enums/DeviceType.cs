using System;

namespace Easy.Common.NetCore.Enums
{
    [Flags]
    public enum DeviceType
    {
        PC = 1,
        Android = 2,
        IOS = 4,
        H5 = 8,
    }
}