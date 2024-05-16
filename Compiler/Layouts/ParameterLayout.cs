using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct ParameterLayout : ILayout<ParameterLayout>
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

        public static ParameterLayout Read(BinaryReader br)
        {
            ParameterLayout self = New(-1, -1);
            self.Type = TypeLayout.Read(br);
            self.Name = br.ReadUtf8();
            return self;
        }
        public static ParameterLayout New(int file, int token) { return new() { FileId = file, TokenId = token }; }
    }
}
