using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
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

        public void add(byte dest, byte op1, byte op2) { Bytes.AddRange([16, dest, op1, op2]); }
        public void sub(byte dest, byte op1, byte op2) { Bytes.AddRange([17, dest, op1, op2]); }
        public void mul(byte dest, byte op1, byte op2) { Bytes.AddRange([18, dest, op1, op2]); }
        public void div(byte dest, byte op1, byte op2) { Bytes.AddRange([19, dest, op1, op2]); }
        public void divu(byte dest, byte op1, byte op2) { Bytes.AddRange([20, dest, op1, op2]); }
        public void rem(byte dest, byte op1, byte op2) { Bytes.AddRange([21, dest, op1, op2]); }
        public void remu(byte dest, byte op1, byte op2) { Bytes.AddRange([22, dest, op1, op2]); }
        public void shl(byte dest, byte op1, byte op2) { Bytes.AddRange([23, dest, op1, op2]); }
        public void shla(byte dest, byte op1, byte op2) { Bytes.AddRange([24, dest, op1, op2]); }
        public void shr(byte dest, byte op1, byte op2) { Bytes.AddRange([25, dest, op1, op2]); }
        public void shra(byte dest, byte op1, byte op2) { Bytes.AddRange([26, dest, op1, op2]); }
        public void and(byte dest, byte op1, byte op2) { Bytes.AddRange([27, dest, op1, op2]); }
        public void or(byte dest, byte op1, byte op2) { Bytes.AddRange([28, dest, op1, op2]); }
        public void xor(byte dest, byte op1, byte op2) { Bytes.AddRange([29, dest, op1, op2]); }

        public void addi(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([32, dest, op1]); AddImm(value); }
        public void subi(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([33, dest, op1]); AddImm(value); }
        public void muli(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([34, dest, op1]); AddImm(value); }
        public void divi(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([35, dest, op1]); AddImm(value); }
        public void shli(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([36, dest, op1]); AddImm(value); }
        public void shlai(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([37, dest, op1]); AddImm(value); }
        public void shri(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([38, dest, op1]); AddImm(value); }
        public void shrai(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([39, dest, op1]); AddImm(value); }
        public void andi(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([40, dest, op1]); AddImm(value); }
        public void ori(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([41, dest, op1]); AddImm(value); }
        public void xori(byte dest, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([42, dest, op1]); AddImm(value); }

        public void jof(short offset)
        {
            Bytes.Add(48);
            Span<byte> bytes = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(bytes, offset);
            Bytes.AddRange(bytes);
        }
        public void jofl(byte label) { Bytes.AddRange([49, label]); }
        public void beq(byte label, byte op1, byte op2) { Bytes.AddRange([50, label, op1, op2]); }
        public void bne(byte label, byte op1, byte op2) { Bytes.AddRange([51, label, op1, op2]); }
        public void bgt(byte label, byte op1, byte op2) { Bytes.AddRange([52, label, op1, op2]); }
        public void blt(byte label, byte op1, byte op2) { Bytes.AddRange([53, label, op1, op2]); }
        public void bge(byte label, byte op1, byte op2) { Bytes.AddRange([54, label, op1, op2]); }
        public void ble(byte label, byte op1, byte op2) { Bytes.AddRange([55, label, op1, op2]); }
        public void bei(byte label, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([56, label, op1]); AddImm(value); }
        public void bni(byte label, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([57, label, op1]); AddImm(value); }
        public void bgi(byte label, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([58, label, op1]); AddImm(value); }
        public void bli(byte label, byte op1, ReadOnlySpan<byte> value) { Bytes.AddRange([59, label, op1]); AddImm(value); }

        public void fadd(byte dest, byte op1, byte op2) { Bytes.AddRange([64, dest, op1, op2]); }
        public void fsub(byte dest, byte op1, byte op2) { Bytes.AddRange([65, dest, op1, op2]); }
        public void fmul(byte dest, byte op1, byte op2) { Bytes.AddRange([66, dest, op1, op2]); }
        public void fdiv(byte dest, byte op1, byte op2) { Bytes.AddRange([67, dest, op1, op2]); }
        public void frem(byte dest, byte op1, byte op2) { Bytes.AddRange([68, dest, op1, op2]); }
        public void fcvt(byte dest, byte from, byte precision) { Bytes.AddRange([69, dest, from, precision]); }
        public void ifcvt(byte dest, byte from, byte precision) { Bytes.AddRange([70, dest, from, precision]); }
        public void ficvt(byte dest, byte from, byte precision) { Bytes.AddRange([71, dest, from, precision]); }

        public void fsqrt(byte dest, byte operand) { Bytes.AddRange([72, dest, operand]); }
        public void fcbrt(byte dest, byte operand) { Bytes.AddRange([73, dest, operand]); }
        public void fsin(byte dest, byte operand) { Bytes.AddRange([74, dest, operand]); }
        public void fcos(byte dest, byte operand) { Bytes.AddRange([75, dest, operand]); }
        public void ftan(byte dest, byte operand) { Bytes.AddRange([76, dest, operand]); }
        public void fasin(byte dest, byte operand) { Bytes.AddRange([77, dest, operand]); }
        public void facos(byte dest, byte operand) { Bytes.AddRange([78, dest, operand]); }
        public void fatan(byte dest, byte operand) { Bytes.AddRange([79, dest, operand]); }
        public void fsinh(byte dest, byte operand) { Bytes.AddRange([80, dest, operand]); }
        public void fcosh(byte dest, byte operand) { Bytes.AddRange([81, dest, operand]); }
        public void ftanh(byte dest, byte operand) { Bytes.AddRange([82, dest, operand]); }
        public void fasinh(byte dest, byte operand) { Bytes.AddRange([83, dest, operand]); }
        public void facosh(byte dest, byte operand) { Bytes.AddRange([84, dest, operand]); }
        public void fatanh(byte dest, byte operand) { Bytes.AddRange([85, dest, operand]); }
        public void fround(byte dest, byte operand) { Bytes.AddRange([86, dest, operand]); }
        public void fceil(byte dest, byte operand) { Bytes.AddRange([87, dest, operand]); }
        public void ffloor(byte dest, byte operand) { Bytes.AddRange([88, dest, operand]); }
        public void fexp(byte dest, byte operand) { Bytes.AddRange([89, dest, operand]); }
        public void fln(byte dest, byte operand) { Bytes.AddRange([90, dest, operand]); }
        public void fpow(byte dest, byte op1, byte op2) { Bytes.AddRange([91, dest, op1, op2]); }

        public void min(byte dest, byte op1, byte op2) { Bytes.AddRange([96, dest, op1, op2]); }
        public void max(byte dest, byte op1, byte op2) { Bytes.AddRange([97, dest, op1, op2]); }

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
        public void starg(byte arg, byte src) { Bytes.AddRange([116, arg, src]); }
        public void ldarg(byte arg, byte dest) { Bytes.AddRange([117, arg, dest]); }

        public void streti(ReadOnlySpan<byte> value) { Bytes.Add(118); AddImm(value); }
        public void stargi(byte arg, ReadOnlySpan<byte> value) { Bytes.AddRange([119, arg]); AddImm(value); }

        public void __Extend() { Bytes.Add(127); }

        public void lk() { Bytes.Add(128); }
        public void ulk() { Bytes.Add(129); }

        public void lda(byte dest, ushort aggsrc, ushort type, ushort index)
        {
            Bytes.AddRange([144, dest, aggsrc.Hi(), aggsrc.Lo(), type.Hi(), type.Lo(), index.Hi(), index.Lo()]);
        }
        public void sta(ushort aggdest, ushort type, ushort index, byte src)
        {
            Bytes.AddRange([145, aggdest.Hi(), aggdest.Lo(), type.Hi(), type.Lo(), index.Hi(), index.Lo(), src]);
        }
    }
}
