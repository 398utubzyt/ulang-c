using System;

namespace Ulang.Layouts
{
    [Flags]
    public enum FunctionModifierFlags : byte
    {
        Hidden = 1,
        Extern = 2,
        Variadic = 4,
        Instance = 8,
    }
}
