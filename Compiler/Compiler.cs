using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Ulang.Layouts;

namespace Ulang
{
    public sealed class Compiler
    {
        private static readonly Dictionary<string, TypeId> _builtins = new() {
            { "i8", TypeId.I8 },
            { "i16", TypeId.I16 },
            { "i32", TypeId.I32 },
            { "i64", TypeId.I64 },
            { "i128", TypeId.I128 },
            { "u8", TypeId.U8 },
            { "u16", TypeId.U16 },
            { "u32", TypeId.U32 },
            { "u64", TypeId.U64 },
            { "u128", TypeId.U128 },
            { "f16", TypeId.F16 },
            { "f32", TypeId.F32 },
            { "f64", TypeId.F64 },
            { "f128", TypeId.F128 },
            { "b8", TypeId.B8 },
            { "b16", TypeId.B16 },
            { "b32", TypeId.B32 },
            { "b64", TypeId.B64 },
            { "c8", TypeId.C8 },
            { "c16", TypeId.C16 },
            { "c32", TypeId.C32 },
            { "isize", TypeId.ISize },
            { "usize", TypeId.USize },
            { "void", TypeId.Void },
            { "noreturn", TypeId.NoReturn },
        };

        private readonly CmdArgs _args;

        

        public static void PrintError(string message)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        private static bool HexToDec(ReadOnlySpan<char> hex, Span<char> dec, out int len)
        {
            if (decimal.TryParse(hex, NumberStyles.HexNumber | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out decimal num))
                return num.TryFormat(dec, out len, default, CultureInfo.InvariantCulture.NumberFormat);
            len = 0;
            return false;
        }

        private static bool BinToDec(ReadOnlySpan<char> bin, Span<char> dec, out int len)
        {
            if (decimal.TryParse(bin, NumberStyles.BinaryNumber | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture.NumberFormat, out decimal num))
                return num.TryFormat(dec, out len, default, CultureInfo.InvariantCulture.NumberFormat);
            len = 0;
            return false;
        }

        public static bool GetNumberLiteralInfo(ReadOnlySpan<char> buffer, Span<byte> bytes, out NumberLiteralType type, out NumberLiteralFormat format)
        {
            type = NumberLiteralType.Unknown;
            format = NumberLiteralFormat.Decimal;

            int precInd = buffer.LastIndexOf("i");
            if (precInd > 0 && !buffer.Contains('.'))
                type = NumberLiteralType.Signed;
            else if ((precInd = buffer.LastIndexOf("u")) > 0 && !buffer.Contains('.'))
                type = NumberLiteralType.Unsigned;
            else if ((precInd = buffer.LastIndexOf("f")) > 0 || buffer.Contains('.'))
                type = NumberLiteralType.Float;
            else
                type = NumberLiteralType.Signed;

            if (precInd > 0 && precInd != buffer.Length - 1)
            {
                ReadOnlySpan<char> prec = buffer[(precInd + 1)..];
                switch (prec)
                {
                    case "8":
                        type++;
                        break;
                    case "16":
                        type += 2;
                        break;
                    case "32":
                        type += 3;
                        break;
                    case "64":
                        type += 4;
                        break;
                    case "128":
                        type += 5;
                        break;

                    default:
                        PrintError($"Invalid number precision: {prec}");
                        return false;
                }
            }

            Span<char> numbuf = stackalloc char[(precInd < 0 ? buffer.Length : precInd)];


            if (buffer.StartsWith("0x"))
                format = NumberLiteralFormat.Hexadecimal;
            else if (buffer.StartsWith("0b"))
                format = NumberLiteralFormat.Binary;

            if (precInd > 0)
                buffer[..precInd].CopyTo(numbuf);
            else
                buffer.CopyTo(numbuf);

            string num = numbuf.ToString();
            switch (type)
            {
                case NumberLiteralType.I8:
                    if (sbyte.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out sbyte i8))
                    {
                        bytes[0] = (byte)i8;
                        return true;
                    }
                    PrintError("Literal is not a valid i8 lvalue.");
                    return false;
                case NumberLiteralType.I16:
                    if (short.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out short i16))
                        return BitConverter.TryWriteBytes(bytes, i16);
                    PrintError("Literal is not a valid i16 lvalue.");
                    return false;
                case NumberLiteralType.I32:
                    if (int.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out int i32))
                        return BitConverter.TryWriteBytes(bytes, i32);
                    PrintError("Literal is not a valid i32 lvalue.");
                    return false;
                case NumberLiteralType.I64:
                    if (long.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out long i64))
                        return BitConverter.TryWriteBytes(bytes, i64);
                    PrintError("Literal is not a valid i64 lvalue.");
                    return false;
                case NumberLiteralType.Signed:
                case NumberLiteralType.I128:
                    if (Int128.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out Int128 i128))
                        return BitConverterEx.TryWriteBytes(bytes, i128);
                    PrintError("Literal is not a valid i128 lvalue.");
                    return false;

                case NumberLiteralType.U8:
                    if (byte.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out byte u8))
                    {
                        bytes[0] = u8;
                        return true;
                    }
                    PrintError("Literal is not a valid i8 lvalue.");
                    return false;
                case NumberLiteralType.U16:
                    if (ushort.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out ushort u16))
                        return BitConverter.TryWriteBytes(bytes, u16);
                    PrintError("Literal is not a valid u16 lvalue.");
                    return false;
                case NumberLiteralType.U32:
                    if (uint.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out uint u32))
                        return BitConverter.TryWriteBytes(bytes, u32);
                    PrintError("Literal is not a valid u32 lvalue.");
                    return false;
                case NumberLiteralType.U64:
                    if (ulong.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out ulong u64))
                        return BitConverter.TryWriteBytes(bytes, u64);
                    PrintError("Literal is not a valid u64 lvalue.");
                    return false;
                case NumberLiteralType.Unknown:
                case NumberLiteralType.Unsigned:
                case NumberLiteralType.U128:
                    if (UInt128.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out UInt128 u128))
                        return BitConverterEx.TryWriteBytes(bytes, u128);
                    PrintError("Literal is not a valid u128 lvalue.");
                    return false;

                case NumberLiteralType.F16:
                    if (Half.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out Half f16))
                        return BitConverter.TryWriteBytes(bytes, f16);
                    PrintError("Literal is not a valid f16 lvalue.");
                    return false;
                case NumberLiteralType.F32:
                    if (float.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out float f32))
                        return BitConverter.TryWriteBytes(bytes, f32);
                    PrintError("Literal is not a valid f32 lvalue.");
                    return false;
                case NumberLiteralType.Float:
                case NumberLiteralType.F64:
                    if (double.TryParse(num, CultureInfo.InvariantCulture.NumberFormat, out double f64))
                        return BitConverter.TryWriteBytes(bytes, f64);
                    PrintError("Literal is not a valid f64 lvalue.");
                    return false;
                case NumberLiteralType.F128:
                    PrintError("Cannot natively parse f128 yet.");
                    return false;
            }

            return false;
        }

        private static int PrefixNamespaceAndRead(StreamReader file, List<NamespaceData> ns, Span<char> s, Token token)
        {
            int nlen = 0;
            for (int i = 0; i < ns.Count; ++i)
            {
                ns[i].Name.CopyTo(s[nlen..]);
                nlen += ns[i].Name.Length;
                s[nlen++] = '.';
            }

            file.Goto(token);
            file.Read(s[nlen..(nlen += token.Length)]);
            return nlen;
        }

        private static bool RegisterSymbols(SourceFile src, StreamReader file, List<StructLayout> structs, List<UnionLayout> unions)
        {
            int t = 0;

            bool external = false;

            int depth = 0;
            Span<char> s = stackalloc char[1024];
            List<NamespaceData> ns = new List<NamespaceData>();

            while ((uint)t < (uint)src.Tokens.Length)
            {
                switch (src.Tokens[t].Type)
                {
                    case TokenType.Separator:
                        if (src.Tokens[t].Data.Separator == SeparatorType.OpenCurly)
                            depth++;
                        else if (src.Tokens[t].Data.Separator == SeparatorType.CloseCurly)
                        {
                            depth--;
                            if (ns.Count > 0 && depth < ns[^1].Depth)
                                ns.RemoveAt(ns.Count - 1);
                        }
                        break;

                    case TokenType.Keyword:
                        switch (src.Tokens[t].Data.Keyword)
                        {
                            case KeywordType.Namespace:
                                if (++t >= src.Tokens.Length - 2)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                if (src.Tokens[t].Type != TokenType.Identifier)
                                {
                                    PrintError("Namespace does not have identifier");
                                    return false;
                                }

                                int nsLength = src.Tokens[t].Length;
                                file.Goto(src.Tokens[t]);
                                file.Read(s[..nsLength]);
                                t++;

                                int nsStart = 0;
                                while (src.Tokens[t].Type == TokenType.Separator && src.Tokens[t].Data.Separator == SeparatorType.Dot)
                                {
                                    while (src.Tokens[++t].Type == TokenType.Identifier)
                                    {
                                        nsStart += nsLength;
                                        s[nsStart++] = '.';
                                        file.Goto(src.Tokens[t]);
                                        file.Read(s[nsStart..(nsStart + nsLength)]);
                                        nsLength = src.Tokens[t++].Length;
                                    }
                                }
                                nsLength += nsStart;

                                if (src.Tokens[t].Type != TokenType.Separator)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                if (src.Tokens[t].Data.Separator == SeparatorType.OpenCurly)
                                    ++depth;
                                else if (src.Tokens[t].Data.Separator != SeparatorType.Semicolon)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                ns.Add(new NamespaceData { Name = new string(s[..nsLength]), Depth = depth });

                                break;

                            case KeywordType.Extern:
                                external = true;
                                break;

                            case KeywordType.Struct:
                                if (external)
                                {
                                    PrintError("Struct cannot be marked as `extern`");
                                    return false;
                                }

                                if (++t >= src.Tokens.Length - 5)
                                {
                                    PrintError("Struct is empty or invalid");
                                    return false;
                                }

                                StructLayout strct = StructLayout.New(src.Id, t - 1);
                                strct.Name = new string(s[..PrefixNamespaceAndRead(file, ns, s, src.Tokens[t])]);
                                structs.Add(strct);
                                break;

                            case KeywordType.Union:
                                if (external)
                                {
                                    PrintError("Union cannot be marked as `extern`");
                                    return false;
                                }

                                if (++t >= src.Tokens.Length - 5)
                                {
                                    PrintError("Union is empty or invalid");
                                    return false;
                                }

                                UnionLayout uni = UnionLayout.New(src.Id, t - 1);
                                uni.Name = new string(s[..PrefixNamespaceAndRead(file, ns, s, src.Tokens[t])]);
                                unions.Add(uni);
                                break;
                        }
                        break;
                }

                t++;
            }

            return true;
        }
        private bool RegisterSymbols(ref IRModule mod, Span<SourceFile> src)
        {
            mod._structs.Clear();
            mod._unions.Clear();
            for (int i = 0; i < src.Length; ++i)
            {
                using FileStream fs = new FileStream(src[i].Path, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new StreamReader(fs);

                if (!RegisterSymbols(src[i], sr, mod._structs, mod._unions))
                    return false;
            }
            return true;
        }

        private static void ToNextMember(SourceFile src, ref int from)
        {
            while (from < src.Tokens.Length && src.Tokens[from].Type != TokenType.Identifier)
                from++;
        }
        private static bool GetTypeFrom(SourceFile src, StreamReader file, Span<char> s,
            List<StructLayout> structs, List<UnionLayout> unions,ref int t, ref TypeLayout type, bool silent = false)
        {
            file.Goto(src.Tokens[t]);
            int cred = file.Read(s[..src.Tokens[t].Length]);

            string typeName = s[..src.Tokens[t].Length].ToString();
            if (!_builtins.TryGetValue(typeName, out TypeId tid))
            {
                tid = TypeId.UserStruct;

                int userType = structs.FindIndex(str => typeName.Equals(str.Name, StringComparison.Ordinal));
                if (userType < 0)
                {
                    tid = TypeId.UserUnion;
                    userType = unions.FindIndex(str => typeName.Equals(str.Name, StringComparison.Ordinal));

                    if (userType < 0)
                    {
                        if (!silent)
                            PrintError($"Could not find type `{typeName}` ({s.Length} {src.Tokens[t].Length}).");
                        return false;
                    }
                }

                type.StructId = (ushort)userType;
            }

            type.Id = tid;

            while (++t < src.Tokens.Length && src.Tokens[t].Type == TokenType.Operator && src.Tokens[t].Data.Operator == OperatorType.Star)
                type.Indir++;

            if (src.Tokens[t].Type == TokenType.Separator && src.Tokens[t].Data.Separator == SeparatorType.OpenSquare)
            {
                if (src.Tokens[++t].Type != TokenType.Literal || src.Tokens[t].Data.Literal != LiteralType.Number)
                {
                    if (!silent)
                        PrintError("Vector length is not a literal value.");
                    return false;
                }

                type.VecIndir = type.Indir;
                type.Indir = 0;

                file.Goto(src.Tokens[t]);
                file.Read(s[..src.Tokens[t].Length]);

                Span<char> lVal = s[..src.Tokens[t].Length];
                Span<byte> rawLValue = stackalloc byte[16]; // Max size of i128
                rawLValue.Clear();

                if (!GetNumberLiteralInfo(lVal, rawLValue, out NumberLiteralType lType, out NumberLiteralFormat lFmt))
                    return false;

                if (lType >= NumberLiteralType.Float)
                {
                    if (!silent)
                        PrintError($"Vector element count cannot be a floating-point number.");
                    return false;
                }

                if ((uint)rawLValue.IndexOf((byte)0) > 3)
                {
                    if (!silent)
                        PrintError($"Vector element count cannot be larger than 16777215.");
                    return false;
                }

                type.VecCount = (UInt24)BitConverter.ToUInt32(rawLValue);

                if (src.Tokens[++t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.CloseSquare)
                {
                    if (!silent)
                        PrintError("Vector element count must be followed by a closing square bracket `]`.");
                    return false;
                }

                while (++t < src.Tokens.Length && src.Tokens[t].Type == TokenType.Operator && src.Tokens[t].Data.Operator == OperatorType.Star)
                    type.Indir++;
            }

            return true;
        }
        private static bool ImplementNextField(Span<SourceFile> src, StreamReader file, Span<char> s,
            List<StructLayout> structs, List<UnionLayout> unions, List<FieldLayout> fields, int fileId, ref int t)
        {
            SourceFile def = src[fileId];

            ToNextMember(def, ref t);
            FieldLayout field = FieldLayout.New(fileId, t);

            TypeLayout type = TypeLayout.New(-1, -1);
            if (!GetTypeFrom(def, file, s, structs, unions, ref t, ref type))
                return false;

            if (def.Tokens[t].Type != TokenType.Identifier)
            {
                PrintError("Type member must be defined as its type immediately followed by its name.");
                return false;
            }

            file.Goto(def.Tokens[t]);
            file.Read(s[..def.Tokens[t].Length]);

            field.Name = s[..def.Tokens[t].Length].ToString();
            field.Type = type;

            if (def.Tokens[++t].Type != TokenType.Separator)
                return false;

            if (def.Tokens[t].Data.Separator != SeparatorType.Semicolon)
            {
                if (def.Tokens[t].Data.Separator != SeparatorType.OpenParen)
                    return false;

                t = def.NextSeparator(SeparatorType.OpenCurly, t) + 1;
                int depth = 1;
                while (depth > 0)
                {
                    if (t >= def.Tokens.Length)
                        return false;

                    if (def.Tokens[t].Type == TokenType.Separator)
                    {
                        if (def.Tokens[t].Data.Separator == SeparatorType.OpenCurly)
                            depth++;
                        else if (def.Tokens[t].Data.Separator == SeparatorType.CloseCurly)
                            depth--;
                    }
                    t++;
                }
                return true;
            }

            fields.Add(field);

            t++;

            return true;
        }
        private static bool ImplementFields(Span<SourceFile> src, StreamReader file, List<StructLayout> structs, List<UnionLayout> unions, ref FieldLayout[] f, int fileId, int start)
        {
            Span<char> s = stackalloc char[1024];
            List<FieldLayout> fields = new List<FieldLayout>();
            int t = src[fileId].NextSeparator(SeparatorType.OpenCurly, start) + 1;

            while (!(src[fileId].Tokens[t].Type == TokenType.Separator && src[fileId].Tokens[t].Data.Separator == SeparatorType.CloseCurly))
            {
                if (!ImplementNextField(src, file, s, structs, unions, fields, fileId, ref t))
                    return false;
            }

            f = fields.ToArray();

            return true;
        }
        private bool ImplementSymbols(ref IRModule mod, Span<SourceFile> src)
        {
            for (int i = 0; i < mod._structs.Count; ++i)
            {
                using FileStream fs = new FileStream(src[mod._structs[i].FileId].Path, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new StreamReader(fs);

                StructLayout cur = mod._structs[i];
                if (!ImplementFields(src, sr, mod._structs, mod._unions, ref cur.Fields, cur.FileId, cur.TokenId))
                    return false;
                mod._structs[i] = cur;
            }

            for (int i = 0; i < mod._unions.Count; ++i)
            {
                using FileStream fs = new FileStream(src[mod._unions[i].FileId].Path, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new StreamReader(fs);

                UnionLayout cur = mod._unions[i];
                if (!ImplementFields(src, sr, mod._structs, mod._unions, ref cur.Fields, cur.FileId, cur.TokenId))
                    return false;
                mod._unions[i] = cur;
            }

            return true;
        }

        private bool RegisterFunctions(SourceFile src, StreamReader file, List<FunctionLayout> functions,
            List<CodeBodyLayout> bodies, List<StructLayout> structs, List<UnionLayout> unions)
        {
            int t = 0;

            FunctionFlags flags = 0;

            int depth = 0;
            Span<char> s = stackalloc char[1024];
            List<NamespaceData> ns = new List<NamespaceData>();

            while ((uint)t < (uint)src.Tokens.Length)
            {
                switch (src.Tokens[t].Type)
                {
                    case TokenType.Separator:
                        if (src.Tokens[t].Data.Separator == SeparatorType.OpenCurly)
                            depth++;
                        else if (src.Tokens[t].Data.Separator == SeparatorType.CloseCurly)
                        {
                            depth--;
                            if (ns.Count > 0 && depth < ns[^1].Depth)
                                ns.RemoveAt(ns.Count - 1);
                        }
                        break;

                    case TokenType.Identifier:
                        if (t >= src.Tokens.Length - 1)
                            return false;

                        TypeLayout type = TypeLayout.New(-1, -1);
                        if (GetTypeFrom(src, file, s, structs, unions, ref t, ref type, true) &&
                            src.Tokens[t + 1].Type == TokenType.Separator &&
                            src.Tokens[t + 1].Data.Separator == SeparatorType.OpenParen)
                        {
                            FunctionLayout layout = FunctionLayout.New(src.Id, t);

                            if (src.Tokens[t].Type != TokenType.Identifier)
                            {
                                PrintError("Function prototype must be defined as its type immediately followed by its name.");
                                return false;
                            }

                            layout.Name = new string(s[..PrefixNamespaceAndRead(file, ns, s, src.Tokens[t++])]);
                            layout.Return = type;

                            if (src.Tokens[t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.CloseParen)
                            {
                                List<ParameterLayout> para = new List<ParameterLayout>();

                                if (src.Tokens[++t].Type == TokenType.Operator)
                                {
                                    if (src.Tokens[t].Data.Operator != OperatorType.Star)
                                        return false;

                                    ushort structId;
                                    bool union = false;

                                    if (ns.Count > 0)
                                    {
                                        string name = string.Join('.', ns.Select((n) => n.Name));
                                        for (int i = 0; i < structs.Count; ++i)
                                            if (structs[i].Name.Equals(name, StringComparison.Ordinal))
                                            {
                                                structId = (ushort)i;
                                                goto InstanceTypeSearchSuccess;
                                            }

                                        union = true;
                                        for (int i = 0; i < unions.Count; ++i)
                                            if (unions[i].Name.Equals(name, StringComparison.Ordinal))
                                            {
                                                structId = (ushort)i;
                                                goto InstanceTypeSearchSuccess;
                                            }
                                    }

                                    PrintError("Instance functions can only exist inside data types. (E.g. structs or unions)");
                                    return false;

                                InstanceTypeSearchSuccess:
                                    flags |= FunctionFlags.Instance;

                                    ParameterLayout p = ParameterLayout.New(src.Id, t++);

                                    p.Type = TypeLayout.New(-1, -1);
                                    p.Type.StructId = structId;
                                    p.Type.Id = union ? TypeId.UserUnion : TypeId.UserStruct;
                                    p.Type.Indir = 1;

                                    file.Goto(src.Tokens[t]);
                                    p.Name = new string(s[..file.Read(s[..src.Tokens[t].Length])]);
                                    para.Add(p);
                                    t++;
                                }

                                while (src.Tokens[t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.CloseParen)
                                {
                                    if (para.Count > 0)
                                    {
                                        if (src.Tokens[t].Type != TokenType.Separator && src.Tokens[t].Data.Separator != SeparatorType.Comma)
                                        {
                                            PrintError("Function parameters must be separated by a comma.");
                                            return false;
                                        }
                                        t++;
                                    }

                                    ParameterLayout p = ParameterLayout.New(src.Id, t);
                                    if (src.Tokens[t].Type == TokenType.Separator && src.Tokens[t].Data.Separator == SeparatorType.Dot)
                                    {
                                        if (src.Tokens[++t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.Dot)
                                            return false;
                                        if (src.Tokens[++t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.Dot)
                                            return false;
                                        if (src.Tokens[++t].Type != TokenType.Separator || src.Tokens[t].Data.Separator != SeparatorType.CloseParen)
                                        {
                                            PrintError("Variadic arguments must be placed last in the parameter list.");
                                            return false;
                                        }

                                        p.Name = "...";
                                        p.Type = TypeLayout.New(-1, -1);
                                        p.Type.Id = TypeId.Variadic;

                                        para.Add(p);

                                        flags |= FunctionFlags.Variadic;
                                        break;
                                    } else
                                    {
                                        type = TypeLayout.New(-1, -1);
                                        if (!GetTypeFrom(src, file, s, structs, unions, ref t, ref type))
                                            return false;

                                        file.Goto(src.Tokens[t]);
                                        p.Name = new string(s[..file.Read(s[..src.Tokens[t++].Length])]);
                                        p.Type = type;
                                        para.Add(p);
                                    }
                                }

                                layout.Params = para.ToArray();
                            } else
                                layout.Params = Array.Empty<ParameterLayout>();

                            if ((flags & FunctionFlags.Extern) == 0)
                            {
                                CodeBodyLayout body = CodeBodyLayout.New(src.Id, src.NextSeparator(SeparatorType.OpenCurly, t) + 1);
                                layout.BodyId = (ushort)bodies.Count;
                                bodies.Add(body);
                            }

                            layout.Flags = flags;
                            functions.Add(layout);
                        }

                        flags = 0;
                        break;

                    case TokenType.Keyword:
                        switch (src.Tokens[t].Data.Keyword)
                        {
                            case KeywordType.Public:
                            case KeywordType.Private:
                            case KeywordType.Protected:
                            case KeywordType.Internal:
                                break;

                            case KeywordType.Namespace:
                            case KeywordType.Struct:
                                if (++t >= src.Tokens.Length - 2)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                if (src.Tokens[t].Type != TokenType.Identifier)
                                {
                                    PrintError("Namespace does not have identifier");
                                    return false;
                                }

                                int nsLength = src.Tokens[t].Length;
                                file.Goto(src.Tokens[t]);
                                file.Read(s[..nsLength]);
                                t++;

                                int nsStart = 0;
                                while (src.Tokens[t].Type == TokenType.Separator && src.Tokens[t].Data.Separator == SeparatorType.Dot)
                                {
                                    while (src.Tokens[++t].Type == TokenType.Identifier)
                                    {
                                        nsStart += nsLength;
                                        s[nsStart++] = '.';
                                        file.Goto(src.Tokens[t]);
                                        file.Read(s[nsStart..(nsStart + nsLength)]);
                                        nsLength = src.Tokens[t++].Length;
                                    }
                                }
                                nsLength += nsStart;

                                if (src.Tokens[t].Type != TokenType.Separator)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                if (src.Tokens[t].Data.Separator == SeparatorType.OpenCurly)
                                    ++depth;
                                else if (src.Tokens[t].Data.Separator != SeparatorType.Semicolon)
                                {
                                    PrintError("Namespace is invalid");
                                    return false;
                                }

                                ns.Add(new NamespaceData { Name = new string(s[..nsLength]), Depth = depth });

                                break;

                            case KeywordType.Extern:
                                flags |= FunctionFlags.Extern;
                                break;
                        }
                        break;
                }

                t++;
            }

            return true;
        }
        private bool RegisterFunctions(ref IRModule mod, Span<SourceFile> src)
        {
            mod._functions.Clear();
            mod._bodies.Clear();
            for (int i = 0; i < src.Length; ++i)
            {
                using FileStream fs = new FileStream(src[i].Path, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new StreamReader(fs);

                if (!RegisterFunctions(src[i], sr, mod._functions, mod._bodies, mod._structs, mod._unions))
                    return false;
            }
            return true;
        }

        
        private bool CompileFunctions(ref IRModule mod, Span<SourceFile> src)
        {
            for (int i = 0; i < mod._functions.Count; ++i)
            {
                FunctionLayout fn = mod._functions[i];
                if ((fn.Flags & FunctionFlags.Extern) == FunctionFlags.Extern)
                {
                    Console.WriteLine($"- {fn.Name} is external. Skipping...");
                    continue;
                }
                Console.WriteLine($"- {fn.Name}");

                using FileStream fs = new FileStream(src[fn.FileId].Path, FileMode.Open, FileAccess.Read);
                using StreamReader sr = new StreamReader(fs);
                using CgContext cg = new CgContext();

                CodeBodyLayout body = mod._bodies[fn.BodyId];

                if (!cg.Process(sr, mod, mod._functions[i], src[fn.FileId].Tokens.AsSpan()[body.TokenId..]))
                {
                    // Print line number here
                    long pos = cg.ErrorPosition;
                    fs.Seek(0, SeekOrigin.Begin);
                    sr.DiscardBufferedData();

                    int line = 0, fpos = 0;
                    string str;
                    while ((str = sr.ReadLine()) != null)
                    {
                        fpos += str.Length;
                        if (fpos >= pos)
                            break;
                        ++line;
                    }

                    PrintError($"    at \"{src[fn.FileId].Path}:{line}\" (char {cg.ErrorPosition})\n{cg.ErrorMessage}");
                    return false;
                } else
                {
                    body.Code = new byte[cg.ByteCount];
                    cg.CopyTo(body.Code);
                    Console.WriteLine(string.Join(' ', body.Code));
                }

                //body.Code = new byte[cg.ByteCount];
                //cg.CopyTo(body.Code);
                mod._bodies[fn.BodyId] = body;
            }
            return true;
        }

        public bool Compile(ref IRModule mod, Span<SourceFile> src)
        {
            Console.WriteLine("Registering type symbols...");
            if (!RegisterSymbols(ref mod, src))
                return false;

            //Console.WriteLine("Registered Struct Symbols:");
            //foreach (StructLayout str in _structs)
            //    Console.WriteLine(str.Name);
            //Console.WriteLine("\nRegistered Union Symbols:");
            //foreach (UnionLayout un in _unions)
            //    Console.WriteLine(un.Name);
            //Console.WriteLine();

            Console.WriteLine("Implementing type symbols...");
            if (!ImplementSymbols(ref mod, src))
                return false;

            //Console.WriteLine("Registered Structs:");
            //foreach (StructLayout str in mod._structs)
            //{
            //    Console.WriteLine($"- {str.Name}:");
            //    foreach (FieldLayout fld in str.Fields)
            //    {
            //        fld.Type.Print(mod, Console.Out);
            //        Console.Write(' ');
            //        Console.WriteLine(fld.Name);
            //    }
            //    Console.WriteLine();
            //}

            //Console.WriteLine("Registered Unions:");
            //foreach (UnionLayout uni in mod._unions)
            //{
            //    Console.WriteLine($"- {uni.Name}:");
            //    foreach (FieldLayout fld in uni.Fields)
            //    {
            //        fld.Type.Print(mod, Console.Out);
            //        Console.Write(' ');
            //        Console.WriteLine(fld.Name);
            //    }
            //    Console.WriteLine();
            //}

            Console.WriteLine("Registering function prototypes...");
            if (!RegisterFunctions(ref mod, src))
                return false;

            //Console.WriteLine("Registered Function Prototypes:");
            //foreach (FunctionLayout fn in mod._functions)
            //{
            //    fn.Return.Print(mod, Console.Out);

            //    Console.Write(' ');
            //    Console.Write(fn.Name);
            //    Console.Write('(');

            //    if (fn.Params.Length > 0)
            //    {
            //        int i = 0;

            //        if ((fn.Flags & FunctionFlags.Instance) == FunctionFlags.Instance)
            //        {
            //            Console.Write('*');
            //            Console.Write(fn.Params[0].Name);

            //            if (i < fn.Params.Length - 1)
            //                Console.Write(", ");
            //            i++;
            //        }

            //        for (; i < fn.Params.Length - 1; ++i)
            //        {
            //            ParameterLayout para = fn.Params[i];
            //            para.Type.Print(mod, Console.Out);
            //            Console.Write(' ');
            //            Console.Write(para.Name);
            //            Console.Write(", ");
            //        }

            //        if ((fn.Flags & FunctionFlags.Variadic) == FunctionFlags.Variadic)
            //            Console.Write("...");
            //        else if (i > 0)
            //        {
            //            fn.Params[^1].Type.Print(mod, Console.Out);
            //            Console.Write(' ');
            //            Console.Write(fn.Params[^1].Name);
            //        }
            //    }

            //    Console.WriteLine(')');
            //}
            //Console.WriteLine();

            Console.WriteLine("Compiling function bodies...");
            if (!CompileFunctions(ref mod, src))
                return false;

            return true;
        }

        

        public Compiler(CmdArgs args)
        {
            _args = args;
        }
    }
}
