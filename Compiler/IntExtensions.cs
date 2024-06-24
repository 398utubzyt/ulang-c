using System;
using System.Buffers.Binary;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ulang
{
    public static class IntExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Hi(this ushort num)
        {
            if (BitConverter.IsLittleEndian)
                return (byte)((num >> 8) & 0xFF);
            else
                return (byte)(num & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Lo(this ushort num)
        {
            if (BitConverter.IsLittleEndian)
                return (byte)(num & 0xFF);
            else
                return (byte)((num >> 8) & 0xFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Hi(this short num)
            => Hi((ushort)num);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Lo(this short num)
            => Lo((ushort)num);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Hi(this uint num)
        {
            if (BitConverter.IsLittleEndian)
                return (ushort)((num >> 16) & 0xFFFF);
            else
                return (ushort)(num & 0xFFFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Lo(this uint num)
        {
            if (BitConverter.IsLittleEndian)
                return (ushort)(num & 0xFFFF);
            else
                return (ushort)((num >> 16) & 0xFFFF);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Hi(this int num)
            => Hi((uint)num);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Lo(this int num)
            => Lo((uint)num);
    }
}
