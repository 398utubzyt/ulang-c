using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct FieldLayout : ILayout<FieldLayout>
    {
        public int FileId;
        public int TokenId;

        public TypeLayout Type;
        public string Name;

        public readonly bool Write(BinaryWriter bw)
        {
            Type.Write(bw);
            bw.WriteUtf8(Name);
            return true;
        }
        public static FieldLayout Read(BinaryReader br)
        {
            FieldLayout self = New(-1, -1);
            self.Type = TypeLayout.Read(br);
            self.Name = br.ReadUtf8();
            return self;
        }
        public static FieldLayout New(int file, int token) { return new() { FileId = file, TokenId = token }; }
    }
}
