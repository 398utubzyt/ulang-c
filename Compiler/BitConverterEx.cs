using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ulang
{
    public static class BitConverterEx
    {
        /// <summary>
        /// Converts a 128-bit signed integer into a span of bytes.
        /// </summary>
        /// <param name="destination">When this method returns, the bytes representing the converted 64-bit signed integer.</param>
        /// <param name="value">The 128-bit signed integer to convert.</param>
        /// <returns><see langword="true"/> if the conversion was successful; <see langword="false"/> otherwise.</returns>
        public static unsafe bool TryWriteBytes(Span<byte> destination, Int128 value)
        {
            if (destination.Length < sizeof(Int128))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }

        /// <summary>
        /// Converts a 128-bit signed integer into a span of bytes.
        /// </summary>
        /// <param name="destination">When this method returns, the bytes representing the converted 64-bit signed integer.</param>
        /// <param name="value">The 128-bit signed integer to convert.</param>
        /// <returns><see langword="true"/> if the conversion was successful; <see langword="false"/> otherwise.</returns>
        public static unsafe bool TryWriteBytes(Span<byte> destination, UInt128 value)
        {
            if (destination.Length < sizeof(UInt128))
                return false;

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
            return true;
        }
    }
}
