using System;
using System.IO;
using System.Text;

using Ulang.Layouts;

namespace Ulang
{
    public static class BinaryIOExtensions
    {
        public static int WriteUtf8Size(this BinaryWriter bw, string value)
        {
            int size = Encoding.UTF8.GetByteCount(value);
            bw.Write((ushort)size);
            return size;
        }
        public static void WriteUtf8Buffer(this BinaryWriter bw, string value, int byteCount)
        {
            Span<byte> b = stackalloc byte[byteCount];
            bw.Write(b[..Encoding.UTF8.GetBytes(value, b)]);
        }
        public static void WriteUtf8(this BinaryWriter bw, string value)
        {
            Span<byte> b = stackalloc byte[WriteUtf8Size(bw, value)];
            bw.Write(b[..Encoding.UTF8.GetBytes(value, b)]);
        }
        public static void WriteArray<T>(this BinaryWriter bw, T[] value) where T : ILayout<T>
        {
            for (int i = 0; i < value.Length; ++i)
                value[i].Write(bw);
        }
        public static void WriteList<T>(this BinaryWriter bw, System.Collections.Generic.List<T> value) where T : ILayout<T>
        {
            for (int i = 0; i < value.Count; ++i)
                value[i].Write(bw);
        }

        public static int ReadUtf8Size(this BinaryReader br)
        {
            return br.ReadUInt16();
        }
        public static string ReadUtf8Buffer(this BinaryReader br, int byteCount)
        {
            Span<byte> b = stackalloc byte[byteCount];
            br.Read(b);
            return Encoding.UTF8.GetString(b);
        }
        public static string ReadUtf8(this BinaryReader br)
        {
            return ReadUtf8Buffer(br, ReadUtf8Size(br));
        }

        public static T[] ReadArraySize8<T>(this BinaryReader br, out byte size) where T : ILayout<T>
            => new T[size = br.ReadByte()];
        public static T[] ReadArraySize16<T>(this BinaryReader br, out ushort size) where T : ILayout<T>
            => new T[size = br.ReadUInt16()];
        public static T[] ReadArraySize32<T>(this BinaryReader br, out uint size) where T : ILayout<T>
            => new T[size = br.ReadUInt32()];
        public static void ReadArray<T>(this BinaryReader br, T[] to) where T : ILayout<T>
        {
            for (int i = 0; i < to.Length; ++i)
                to[i] = T.Read(br);
        }
        public static void ReadList<T>(this BinaryReader br, System.Collections.Generic.List<T> to) where T : ILayout<T>
        {
            for (int i = 0; i < to.Capacity; ++i)
                to.Add(T.Read(br));
        }
    }
}
