using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct StructLayout : ILayout<StructLayout>
    {
        public int FileId;
        public int TokenId;

        public string Name;
        public FieldLayout[] Fields;

        public readonly bool Write(BinaryWriter bw)
        {
            bw.Write((ushort)Fields.Length);
            bw.WriteUtf8(Name);
            bw.WriteArray(Fields);
            return true;
        }

        public static StructLayout Read(BinaryReader br)
        {
            StructLayout self = New(-1, -1);
            self.Fields = br.ReadArraySize16<FieldLayout>(out _);
            self.Name = br.ReadUtf8();
            br.ReadArray(self.Fields);
            return self;
        }
        public static StructLayout New(int file, int token) { return new() { FileId = file, TokenId = token }; }
    }
}
