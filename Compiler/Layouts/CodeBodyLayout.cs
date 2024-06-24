using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct CodeBodyLayout : ILayout<CodeBodyLayout>
    {
        public int FileId;
        public int TokenId;

        public byte[] Code;

        public readonly bool Write(BinaryWriter bw)
        {
            bw.Write((ushort)(Code?.Length ?? 0));
            if (Code != null)
                bw.Write(Code);
            return true;
        }

        public static CodeBodyLayout Read(BinaryReader br)
        {
            CodeBodyLayout self = new();

            return self;
        }
        public static CodeBodyLayout New(int file, int pos) { return new() { FileId = file, TokenId = pos }; }
    }
}
