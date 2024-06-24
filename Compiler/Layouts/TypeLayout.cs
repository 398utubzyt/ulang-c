using System;
using System.IO;

namespace Ulang.Layouts
{
    public struct TypeLayout : ILayout<TypeLayout>
    {
        public TypeId Id;
        public byte Indir;
        public ushort StructId;
        public UInt24 VecCount;
        public byte VecIndir;

        public readonly bool Write(BinaryWriter bw)
        {
            bw.Write((byte)Id);
            bw.Write(Indir);
            bw.Write(StructId);
            bw.Write(VecCount);
            bw.Write(VecIndir);
            return true;
        }
        public static TypeLayout Read(BinaryReader br)
        {
            TypeLayout self;
            self.Id = (TypeId)br.ReadByte();
            self.Indir = br.ReadByte();
            self.StructId = br.ReadUInt16();
            self.VecCount = br.ReadUInt24();
            self.VecIndir = br.ReadByte();
            return self;
        }
        public static TypeLayout New(int file, int token) { return default; }

        public static bool operator ==(TypeLayout a, TypeLayout b)
            => a.Id == b.Id && a.Indir == b.Indir && a.StructId == b.StructId && a.VecCount == b.VecCount && a.VecIndir == b.VecIndir;
        public static bool operator !=(TypeLayout a, TypeLayout b)
            => a.Id != b.Id || a.Indir != b.Indir || a.StructId != b.StructId || a.VecCount != b.VecCount || a.VecIndir != b.VecIndir;

        public readonly void Print(IRModule mod, TextWriter sw)
        {
            if (Id == TypeId.UserStruct)
                sw.Write(mod._structs[StructId].Name);
            else if (Id == TypeId.UserUnion)
                sw.Write(mod._unions[StructId].Name);
            else
                sw.Write(Id.ToString());

            if (VecCount > 0)
            {
                for (int j = 0; j < VecIndir; ++j)
                    sw.Write('*');
                sw.Write('[');
                sw.Write(VecCount);
                sw.Write(']');
            }

            for (int j = 0; j < Indir; ++j)
                sw.Write('*');
        }

        public override readonly bool Equals(object obj)
            => obj is TypeLayout l && this == l;

        public override readonly int GetHashCode()
            => Id.GetHashCode();
    }
}
