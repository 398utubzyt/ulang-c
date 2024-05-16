using System.IO;

namespace Ulang.Layouts
{
    public struct FunctionLayout : ILayout<FunctionLayout>
    {
        public int FileId;
        public int TokenId;

        public FunctionFlags Flags;
        public TypeLayout Return;
        public ushort BodyId;
        public string Name;
        public ParameterLayout[] Params;

        public readonly bool Write(BinaryWriter bw)
        {
            bw.Write((byte)Flags);
            Return.Write(bw);
            bw.Write((byte)Params.Length);
            bw.Write(BodyId);
            bw.WriteUtf8(Name);
            return true;
        }

        public static FunctionLayout Read(BinaryReader br)
        {
            FunctionLayout self = New(-1, -1);
            self.Flags = (FunctionFlags)br.ReadByte();
            self.Return = TypeLayout.Read(br);
            self.Params = br.ReadArraySize8<ParameterLayout>(out _);
            self.BodyId = br.ReadUInt16();
            self.Name = br.ReadUtf8();
            br.ReadArray(self.Params);
            return self;
        }
        public static FunctionLayout New(int file, int token) { return new() { FileId = file, TokenId = token }; }
    }
}
