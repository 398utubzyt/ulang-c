using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ulang.Layouts;

namespace Ulang
{
    public static class Codegen
    {
        private enum RegisterPrecision
        {
            I8 = 0,
            I16 = 1,
            I32 = 2,
            I64 = 3,
            I128 = 4,
            IP = 6,

            Float = 16,
            F16 = 17,
            F32 = 18,
            F64 = 19,
            F128 = 20,
            FP = 22,
        }
        private class CodegenState
        {
            private class RegisterSet
            {
                private struct RegisterInfo
                {
                    public RegisterPrecision Precision;
                    public string Name;
                }

                private readonly Dictionary<string, byte> _name2reg = new();
                private readonly RegisterInfo[] _reg2info = new RegisterInfo[256];

                public byte Used => (byte)_name2reg.Count;

                public void Store(byte register, string name, RegisterPrecision precision)
                {
                    if (_reg2info[register].Name != null)
                        _name2reg.Remove(_reg2info[register].Name);
                    _reg2info[register] = new RegisterInfo { Precision = precision, Name = name };
                    _name2reg[name] = register;
                }
                public string GetName(byte register)
                    => _reg2info[register].Name;
                public RegisterPrecision GetPrecision(byte register)
                    => _reg2info[register].Precision;
                public RegisterPrecision GetPrecision(string name)
                    => _reg2info[_name2reg[name]].Precision;
                public byte GetRegister(string name)
                    => _name2reg[name];
                public bool Has(string name)
                    => _name2reg.ContainsKey(name);
                public bool HasPrecision(string name, RegisterPrecision precision)
                    => _name2reg.ContainsKey(name) && _reg2info[_name2reg[name]].Precision == precision;
                public bool TryGetRegister(string name, out byte register)
                    => _name2reg.TryGetValue(name, out register);
            }

            private readonly Dictionary<string, byte> _labels = new();
            private readonly RegisterSet _fields = new();

            public int LabelCount => _labels.Count;
            public void AddLabel(string name)
                => _labels[name] = (byte)_labels.Count;
            public bool HasLabel(string name)
                => _labels.ContainsKey(name);
            public byte GetLabel(string name)
                => _labels[name];
            public bool TryGetLabel(string name, out byte label)
                => _labels.TryGetValue(name, out label);

            public byte UsedFields => _fields.Used;
            public void SetField(byte register, string name, RegisterPrecision precision)
                => _fields.Store(register, name, precision);
            public string GetFieldName(byte register)
                => _fields.GetName(register);
            public RegisterPrecision GetFieldPrecision(byte register)
                => _fields.GetPrecision(register);
            public RegisterPrecision GetFieldPrecision(string name)
                => _fields.GetPrecision(name);
            public byte GetFieldRegister(string name)
                => _fields.GetRegister(name);
            public bool HasField(string name)
                => _fields.Has(name);
            public bool HasFieldPrecision(string name, RegisterPrecision precision)
                => _fields.Has(name);
            public bool TryGetFieldRegister(string name, out byte register)
                => _fields.TryGetRegister(name, out register);
        }

        private static bool DefineLabel(InstructionList inst, ref CodegenState state, ref IRModule mod, StreamReader file, CmdArgs args,
            ReadOnlySpan<Token> src, ref FunctionLayout fn, ref int t, Span<char> s)
        {
            file.Goto(src[t]);
            state.AddLabel(s[..file.Read(s[..src[t].Length])].ToString());
            t += 2;
            return true;
        }

        private static bool EvaluateOperator(InstructionList inst, ref CodegenState state, ref IRModule mod, StreamReader file, CmdArgs args,
            ReadOnlySpan<Token> src, ref FunctionLayout fn, ref int t, Span<char> s)
        {
            while ((uint)t < (uint)src.Length)
            {
                switch (src[t].Type)
                {
                    case TokenType.Separator:
                        switch (src[t].Data.Separator)
                        {
                            case SeparatorType.OpenParen:
                                t++;
                                if (!EvaluateExpression(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                            case SeparatorType.CloseParen:
                            case SeparatorType.Semicolon:
                                if (src[t - 1].Type == TokenType.Operator)
                                {
                                    Compiler.PrintError("Incomplete expression.");
                                    return false;
                                }
                                return true;
                        }
                        break;

                    case TokenType.Identifier:

                        break;

                    case TokenType.Keyword:
                        switch (src[t].Data.Keyword)
                        {
                            case KeywordType.Return:
                                if (!Return(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                        }
                        break;
                }

                t++;
            }
            return true;
        }
        private static bool EvaluateExpression(InstructionList inst, ref CodegenState state, ref IRModule mod, StreamReader file, CmdArgs args,
            ReadOnlySpan<Token> src, ref FunctionLayout fn, ref int t, Span<char> s)
        {
            while ((uint)t < (uint)src.Length)
            {
                switch (src[t].Type)
                {
                    case TokenType.Separator:
                        switch (src[t].Data.Separator)
                        {
                            case SeparatorType.OpenParen:
                                t++;
                                if (!EvaluateExpression(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                            case SeparatorType.CloseParen:
                            case SeparatorType.Semicolon:
                                if (src[t - 1].Type == TokenType.Operator)
                                {
                                    Compiler.PrintError("Incomplete expression.");
                                    return false;
                                }
                                return true;
                        }
                        break;

                    case TokenType.Identifier:

                        break;

                    case TokenType.Keyword:
                        switch (src[t].Data.Keyword)
                        {
                            case KeywordType.Return:
                                if (!Return(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                        }
                        break;
                }

                t++;
            }
            return true;
        }
        private static bool Return(InstructionList inst, ref CodegenState state, ref IRModule mod, StreamReader file, CmdArgs args,
            ReadOnlySpan<Token> src, ref FunctionLayout fn, ref int t, Span<char> s)
        {
            void IntRetType(ref int t)
            {
                ;
            }

            string fname;
            byte reg;

            t++;

            switch (fn.Return.Id)
            {
                case TypeId.NoReturn:
                    Compiler.PrintError("Cannot return from a `noreturn` function.");
                    break;

                case TypeId.I8:
                case TypeId.U8:
                    if (src[t].Type != TokenType.Identifier)
                        break;
                    file.Goto(src[t]);
                    fname = s[..file.Read(s)].ToString();
                    if (!state.HasFieldPrecision(fname, RegisterPrecision.I8) ||
                        !state.TryGetFieldRegister(fname.ToString(), out reg))
                        break;
                    inst.stret(reg);
                    goto FinalSemicolonCheck;
                case TypeId.I16:
                case TypeId.U16:
                    if (src[t].Type != TokenType.Identifier)
                        break;
                    file.Goto(src[t]);
                    fname = s[..file.Read(s)].ToString();
                    if (!state.HasFieldPrecision(fname, RegisterPrecision.I16) ||
                        !state.TryGetFieldRegister(fname.ToString(), out reg))
                        break;
                    goto FinalSemicolonCheck;
                case TypeId.I32:
                case TypeId.U32:
                    if (src[t].Type != TokenType.Identifier)
                    {
                        if (src[t].Type == TokenType.Literal)
                        {
                            
                            goto FinalSemicolonCheck;
                        } else
                            break;
                    }
                    file.Goto(src[t]);
                    fname = s[..file.Read(s)].ToString();
                    if (!state.HasFieldPrecision(fname, RegisterPrecision.I32) ||
                        !state.TryGetFieldRegister(fname.ToString(), out reg))
                    {
                        Compiler.PrintError($"Cannot find register with name \"{fname}\"");
                        break;
                    }
                    goto FinalSemicolonCheck;
                case TypeId.I64:
                case TypeId.U64:
                    if (src[t].Type != TokenType.Identifier)
                        break;
                    file.Goto(src[t]);
                    fname = s[..file.Read(s)].ToString();
                    if (!state.HasFieldPrecision(fname, RegisterPrecision.I64) ||
                        !state.TryGetFieldRegister(fname.ToString(), out reg))
                        break;
                    goto FinalSemicolonCheck;
                case TypeId.I128:
                case TypeId.U128:
                    if (src[t].Type != TokenType.Identifier)
                        break;
                    file.Goto(src[t]);
                    fname = s[..file.Read(s)].ToString();
                    if (!state.HasFieldPrecision(fname, RegisterPrecision.I128) ||
                        !state.TryGetFieldRegister(fname.ToString(), out reg))
                        break;

                    FinalSemicolonCheck:
                    inst.stret(reg);
                    if (src[++t].Type == TokenType.Separator && src[t].Data.Separator == SeparatorType.Semicolon)
                    {
                        inst.ret();
                        return true;
                    }
                    break;

                case TypeId.Void:
                    if (src[++t].Type == TokenType.Separator && src[t].Data.Separator == SeparatorType.Semicolon)
                    {
                        inst.ret();
                        return true;
                    }
                    break;
            }

            Compiler.PrintError($"Invalid return type: {fn.Return.Id}");
            return false;
        }

        public static bool Generate(ref IRModule mod, StreamReader file, CmdArgs args, ReadOnlySpan<Token> src, ref FunctionLayout fn, ref CodeBodyLayout body)
        {
            int t = body.TokenId;
            int depth = 0;
            InstructionList inst = new InstructionList();
            CodegenState state = new CodegenState();
            Span<char> s = stackalloc char[256];

            while ((uint)t < (uint)src.Length && depth >= 0)
            {
                switch (src[t].Type)
                {
                    case TokenType.Separator:
                        switch (src[t].Data.Separator)
                        {
                            case SeparatorType.OpenCurly:
                                depth++;
                                break;
                            case SeparatorType.CloseCurly:
                                depth--;
                                break;
                            case SeparatorType.OpenParen:
                                t++;
                                if (!EvaluateExpression(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                        }
                        break;

                    case TokenType.Identifier:
                        if (src[t + 1].Type == TokenType.Separator)
                        {
                            if (src[t + 1].Data.Separator == SeparatorType.Colon)
                            {
                                if (!DefineLabel(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                            }
                        }
                        break;

                    case TokenType.Keyword:
                        switch (src[t].Data.Keyword)
                        {
                            case KeywordType.Return:
                                if (!Return(inst, ref state, ref mod, file, args, src, ref fn, ref t, s))
                                    return false;
                                break;
                        }
                        break;
                }

                t++;
            }
            body.Code = inst.Bytes.ToArray();

            return true;
        }
    }
}
