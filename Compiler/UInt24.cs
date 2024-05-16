using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ulang
{
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public unsafe struct UInt24 : IComparable<UInt24>, IEquatable<UInt24>
    {
        public const int ByteCount = 3;

        [FieldOffset(0)]
        private fixed byte _bytes[ByteCount];

        public ref byte GetReference() => ref _bytes[0];
        public Span<byte> Bytes => MemoryMarshal.CreateSpan(ref GetReference(), ByteCount);

        public static explicit operator int(UInt24 x)
        {
            int value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned((byte*)&value + 1, x._bytes, ByteCount);
            else
                Unsafe.CopyBlockUnaligned(&value, x._bytes, ByteCount);
            return value;
        }
        public static implicit operator UInt24(sbyte x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                value._bytes[2] = (byte)x;
            else
                value._bytes[0] = (byte)x;
            return value;
        }
        public static implicit operator UInt24(byte x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                value._bytes[2] = x;
            else
                value._bytes[0] = x;
            return value;
        }
        public static implicit operator UInt24(short x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes + 1, &x, 2);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, 2);
            return value;
        }
        public static implicit operator UInt24(ushort x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes + 1, &x, 2);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, 2);
            return value;
        }
        public static implicit operator UInt24(int x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes, (byte*)&x + 1, ByteCount);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, ByteCount);
            return value;
        }
        public static implicit operator UInt24(uint x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes, (byte*)&x + 1, ByteCount);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, ByteCount);
            return value;
        }
        public static explicit operator UInt24(long x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes, (byte*)&x + 5, ByteCount);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, ByteCount);
            return value;
        }
        public static explicit operator UInt24(ulong x)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes, (byte*)&x + 5, ByteCount);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, ByteCount);
            return value;
        }

        public static bool operator >(UInt24 a, int b)
            => (int)a > b;
        public static bool operator <(UInt24 a, int b)
            => (int)a < b;
        public static bool operator >(UInt24 a, UInt24 b)
            => (int)a > (int)b;
        public static bool operator <(UInt24 a, UInt24 b)
            => (int)a < (int)b;
        public static bool operator ==(UInt24 a, UInt24 b)
            => (int)a == (int)b;
        public static bool operator !=(UInt24 a, UInt24 b)
            => (int)a != (int)b;

        private static UInt24 ConvertFrom<T>(T x) where T : unmanaged
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
                Unsafe.CopyBlockUnaligned(value._bytes, ((byte*)&x) + (sizeof(T) - ByteCount), ByteCount);
            else
                Unsafe.CopyBlockUnaligned(value._bytes, &x, ByteCount);
            return value;
        }

        private UInt24(Span<byte> bytes) { bytes.TryCopyTo(Bytes); }

        public readonly override string ToString()
            => ((int)this).ToString();
        public readonly override int GetHashCode()
            => (int)this;
        public readonly override bool Equals([NotNullWhen(true)] object obj)
            => obj is UInt24 && Equals((UInt24)obj);

        public readonly int CompareTo(UInt24 other)
        {
            if (this < other) return -1;
            if (this > other) return 1;
            return 0;
        }
        public readonly bool Equals(UInt24 other)
            => this == other;
    }

    public static class UInt24Extensions
    {
        public static void Write(this BinaryWriter bw, UInt24 value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Span<byte> bytes = value.Bytes;
                bytes.Reverse();
                bw.Write(bytes);
            } else
            {
                bw.Write(value.Bytes);
            }
        }
        public static UInt24 ReadUInt24(this BinaryReader br)
        {
            UInt24 value;
            if (!BitConverter.IsLittleEndian)
            {
                Span<byte> bytes = value.Bytes;
                br.Read(bytes);
                bytes.Reverse();
            } else
            {
                br.Read(value.Bytes);
            }

            return value;
        }
    }
}
