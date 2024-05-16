using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct InstructionLayout : ILayout<InstructionLayout>
    {
        public const byte ExtensionCode = 0x7F;

        public byte Code;
        public byte Extent;

        public static implicit operator InstructionLayout(byte op)
            => new InstructionLayout { Code = op };
        public static implicit operator InstructionLayout(byte[] op)
            => new InstructionLayout { Code = op[^1], Extent = (byte)op.Length };

        public readonly bool Write(BinaryWriter bw)
        {
            for (byte i = 0; i < Extent; ++i)
                bw.Write(ExtensionCode);
            bw.Write(Code);
            return true;
        }

        public static InstructionLayout Read(BinaryReader br)
        {
            InstructionLayout self;
            self.Extent = 0;
            while ((self.Code = br.ReadByte()) != ExtensionCode)
                ++self.Extent;
            return self;
        }
        public static InstructionLayout New(int file, int token) { return default; }
    }
}
