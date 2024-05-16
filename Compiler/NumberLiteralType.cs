using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public enum NumberLiteralType
    {
        Unknown = 0x00,

        Signed = 0x10,
        I8 = 0x11,
        I16 = 0x12,
        I32 = 0x13,
        I64 = 0x14,
        I128 = 0x15,

        Unsigned = 0x20,
        U8 = 0x21,
        U16 = 0x22,
        U32 = 0x23,
        U64 = 0x24,
        U128 = 0x25,

        Float = 0x30,
        F16 = 0x32,
        F32 = 0x33,
        F64 = 0x34,
        F128 = 0x35,
    }
}
