#if UNITY_EDITOR
using System;

namespace PSS
{
    [Flags]
    public enum BVHNodeFlags
    {
        None = 0,
        Root = 0x1,
        Terminal = 0x2
    }
}
#endif