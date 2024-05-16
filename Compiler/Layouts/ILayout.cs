using System.IO;

namespace Ulang.Layouts
{
    public interface ILayout<T> where T : ILayout<T>
    {
        public bool Write(BinaryWriter bw);
        public static abstract T Read(BinaryReader br);
        public static abstract T New(int file, int token);
    }
}
