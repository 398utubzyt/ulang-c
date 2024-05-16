using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public enum InstOp : byte
    {
        Nop = 0x00,
        Break = 0x01,

        Ld8 = 0x02,
        Ld16 = 0x03,
        Ld32 = 0x04,
        Ld64 = 0x05,
        Ld128 = 0x06,
        LdPtr = 0x08,

        St8 = 0x09,
        St16 = 0x0A,
        St32 = 0x0B,
        St64 = 0x0C,
        St128 = 0x0D,
        StPtr = 0x0F,

        Add = 0x10,
    }

    internal class InstructionList
    {
        public List<byte> Bytes = new();
        private void AddImm(ReadOnlySpan<byte> value) { Bytes.Add((byte)value.Length); Bytes.AddRange(value);  }

        public void nop() { Bytes.Add(0); }
        public void dbreak() { Bytes.Add(1); }

        public void ld8(byte dest, byte src) { Bytes.AddRange([2, dest, src]); }
        public void ld16(byte dest, byte src) { Bytes.AddRange([3, dest, src]); }
        public void ld32(byte dest, byte src) { Bytes.AddRange([4, dest, src]); }
        public void ld64(byte dest, byte src) { Bytes.AddRange([5, dest, src]); }
        public void ld128(byte dest, byte src) { Bytes.AddRange([6, dest, src]); }
        public void ldPtr(byte dest, byte src) { Bytes.AddRange([8, dest, src]); }

        public void st8(byte dest, byte src) { Bytes.AddRange([9, dest, src]); }
        public void st16(byte dest, byte src) { Bytes.AddRange([10, dest, src]); }
        public void st32(byte dest, byte src) { Bytes.AddRange([11, dest, src]); }
        public void st64(byte dest, byte src) { Bytes.AddRange([12, dest, src]); }
        public void st128(byte dest, byte src) { Bytes.AddRange([13, dest, src]); }
        public void stPtr(byte dest, byte src) { Bytes.AddRange([15, dest, src]); }

        public void add(byte op1, byte op2) { Bytes.AddRange([16, op1, op2]); }
        public void sub(byte op1, byte op2) { Bytes.AddRange([17, op1, op2]); }
        public void mul(byte op1, byte op2) { Bytes.AddRange([18, op1, op2]); }
        public void div(byte op1, byte op2) { Bytes.AddRange([19, op1, op2]); }
        public void divu(byte op1, byte op2) { Bytes.AddRange([20, op1, op2]); }
        public void rem(byte op1, byte op2) { Bytes.AddRange([21, op1, op2]); }
        public void remu(byte op1, byte op2) { Bytes.AddRange([22, op1, op2]); }
        public void shl(byte op1, byte op2) { Bytes.AddRange([23, op1, op2]); }
        public void shla(byte op1, byte op2) { Bytes.AddRange([24, op1, op2]); }
        public void shr(byte op1, byte op2) { Bytes.AddRange([25, op1, op2]); }
        public void shra(byte op1, byte op2) { Bytes.AddRange([26, op1, op2]); }
        public void and(byte op1, byte op2) { Bytes.AddRange([27, op1, op2]); }
        public void or(byte op1, byte op2) { Bytes.AddRange([28, op1, op2]); }
        public void xor(byte op1, byte op2) { Bytes.AddRange([29, op1, op2]); }

        public void addi(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([32, op1]); AddImm(value); }
        public void subi(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([33, op1]); AddImm(value); }
        public void muli(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([34, op1]); AddImm(value); }
        public void divi(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([35, op1]); AddImm(value); }
        public void shli(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([36, op1]); AddImm(value); }
        public void shlai(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([37, op1]); AddImm(value); }
        public void shri(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([38, op1]); AddImm(value); }
        public void shrai(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([39, op1]); AddImm(value); }
        public void andi(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([40, op1]); AddImm(value); }
        public void ori(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([41, op1]); AddImm(value); }
        public void xori(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([42, op1]); AddImm(value); }

        public void jof(short offset)
        {
            Bytes.Add(48);
            Span<byte> bytes = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(bytes, offset);
            Bytes.AddRange(bytes);
        }
        public void jofl(byte label) { Bytes.AddRange([49, label]); }
        public void beq(byte op1, byte op2) { Bytes.AddRange([50, op1, op2]); }
        public void bne(byte op1, byte op2) { Bytes.AddRange([51, op1, op2]); }
        public void bgt(byte op1, byte op2) { Bytes.AddRange([52, op1, op2]); }
        public void blt(byte op1, byte op2) { Bytes.AddRange([53, op1, op2]); }
        public void bge(byte op1, byte op2) { Bytes.AddRange([54, op1, op2]); }
        public void ble(byte op1, byte op2) { Bytes.AddRange([55, op1, op2]); }
        public void bei(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([56, op1]); AddImm(value); }
        public void bni(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([57, op1]); AddImm(value); }
        public void bgi(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([58, op1]); AddImm(value); }
        public void bli(byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([59, op1]); AddImm(value); }

        public void fadd(byte op1, byte op2) { Bytes.AddRange([64, op1, op2]); }
        public void fsub(byte op1, byte op2) { Bytes.AddRange([65, op1, op2]); }
        public void fmul(byte op1, byte op2) { Bytes.AddRange([66, op1, op2]); }
        public void fdiv(byte op1, byte op2) { Bytes.AddRange([67, op1, op2]); }
        public void frem(byte op1, byte op2) { Bytes.AddRange([68, op1, op2]); }
        public void fcvt(byte dest, byte from, byte precision) { Bytes.AddRange([69, dest, from, precision]); }
        public void ifcvt(byte dest, byte from, byte precision) { Bytes.AddRange([70, dest, from, precision]); }
        public void ficvt(byte dest, byte from, byte precision) { Bytes.AddRange([71, dest, from, precision]); }

        public void fsqrt(byte dest, byte operand) { Bytes.AddRange([72, dest, operand]); }
        public void fsin(byte dest, byte operand) { Bytes.AddRange([75, dest, operand]); }
        public void fcos(byte dest, byte operand) { Bytes.AddRange([76, dest, operand]); }
        public void ftan(byte dest, byte operand) { Bytes.AddRange([77, dest, operand]); }
        public void fasin(byte dest, byte operand) { Bytes.AddRange([78, dest, operand]); }
        public void facos(byte dest, byte operand) { Bytes.AddRange([79, dest, operand]); }
        public void fatan(byte dest, byte operand) { Bytes.AddRange([80, dest, operand]); }
        public void fsinh(byte dest, byte operand) { Bytes.AddRange([81, dest, operand]); }
        public void fcosh(byte dest, byte operand) { Bytes.AddRange([82, dest, operand]); }
        public void ftanh(byte dest, byte operand) { Bytes.AddRange([83, dest, operand]); }
        public void fasinh(byte dest, byte operand) { Bytes.AddRange([84, dest, operand]); }
        public void facosh(byte dest, byte operand) { Bytes.AddRange([85, dest, operand]); }
        public void fatanh(byte dest, byte operand) { Bytes.AddRange([86, dest, operand]); }
        public void fcbrt(byte dest, byte operand) { Bytes.AddRange([87, dest, operand]); }
        public void fround(byte dest, byte operand) { Bytes.AddRange([88, dest, operand]); }
        public void fceil(byte dest, byte operand) { Bytes.AddRange([89, dest, operand]); }
        public void ffloor(byte dest, byte operand) { Bytes.AddRange([90, dest, operand]); }
        public void fexp(byte dest, byte operand) { Bytes.AddRange([91, dest, operand]); }
        public void fln(byte dest, byte operand) { Bytes.AddRange([93, dest, operand]); }
        public void fpow(byte dest, byte op1, byte op2) { Bytes.AddRange([94, dest, op1, op2]); }

        public void call(ushort func)
        {
            Bytes.Add(112);
            Span<byte> bytes = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(bytes, func);
            Bytes.AddRange(bytes);
        }
        public void ret() { Bytes.Add(113); }

        public void stret(byte src) { Bytes.AddRange([114, src]); }
        public void ldret(byte dest) { Bytes.AddRange([115, dest]); }
        public void starg(byte src) { Bytes.AddRange([116, src]); }
        public void ldarg(byte dest) { Bytes.AddRange([117, dest]); }

        public void streti(byte src, ReadOnlySpan<byte> value) { Bytes.AddRange([118, src]); AddImm(value); }
        public void stargi(byte src, ReadOnlySpan<byte> value) { Bytes.AddRange([119, src]); AddImm(value); }

        public void __Extend() { Bytes.Add(127); }

        public void lk() { Bytes.Add(128); }
        public void ulk() { Bytes.Add(129); }
    }
}
