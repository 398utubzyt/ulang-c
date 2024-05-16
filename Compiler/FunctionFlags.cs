namespace Ulang
{
    [System.Flags]
    public enum FunctionFlags : byte
    {
        Hidden = 1,
        Extern = 2,
        Variadic = 4,
        Instance = 8,
        Compt = 16,
        Unsafe = 32,
    }
}
