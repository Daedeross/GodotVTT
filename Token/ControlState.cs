using System;

namespace GodotVTT
{
    [Flags]
    public enum ControlState
    {
        None = 0,
        Primary = 1,
        Secondary = 1 << 1
    }
}