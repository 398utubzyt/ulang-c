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
        ISize = 0x17,

        Unsigned = 0x20,
        U8 = 0x21,
        U16 = 0x22,
        U32 = 0x23,
        U64 = 0x24,
        U128 = 0x25,
        USize = 0x27,

        Float = 0x30,
        F16 = 0x32,
        F32 = 0x33,
        F64 = 0x34,
        F128 = 0x35,
        // FSize = 0x37,

        PrecisionMask = 0x0F,
        Bit8 = 0x01,
        Bit16 = 0x02,
        Bit32 = 0x03,
        Bit64 = 0x04,
        Bit128 = 0x05,
    }

    public static class NumberLiteralTypeExtensions
    {
        public static int GetByteSize(this NumberLiteralType value)
            => (value & NumberLiteralType.PrecisionMask) switch
            {
                NumberLiteralType.Bit8 => 1,
                NumberLiteralType.Bit16 => 2,
                NumberLiteralType.Bit32 => 4,
                NumberLiteralType.Bit64 => 8,
                NumberLiteralType.Bit128 => 16,
                _ => 16,
            };
    }
}
