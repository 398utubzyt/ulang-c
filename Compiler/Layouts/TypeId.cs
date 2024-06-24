using System;

namespace Ulang.Layouts
{
    public enum TypeId : byte
    {
        UserStruct = 0x00,
        UserUnion = 0x01,
        I8 = 0x11,
        I16 = 0x12,
        I32 = 0x13,
        I64 = 0x14,
        I128 = 0x15,
        U8 = 0x21,
        U16 = 0x22,
        U32 = 0x23,
        U64 = 0x24,
        U128 = 0x25,
        F16 = 0x32,
        F32 = 0x33,
        F64 = 0x34,
        F128 = 0x35,
        B8 = 0x41,
        B16 = 0x42,
        B32 = 0x43,
        B64 = 0x44,
        C8 = 0x51,
        C16 = 0x52,
        C32 = 0x53,
        ISize = 0x70,
        USize = 0x71,
        Void = 0x80,
        NoReturn = 0x81,
        Variadic = 0x82,
    }
}
