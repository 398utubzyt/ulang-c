using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

using Ulang.Layouts;

namespace Ulang
{
    public sealed class CgContext : IDisposable
    {
        private bool _dispose;

        private readonly SyContext _sy = new();
        private readonly InstructionList _ir = new();
        private readonly RegisterSim _reg = new();

        private bool _quit;
        private bool _err;
        private string _errMsg;

        private int _scopeTrack;

        public bool HasError => _err || _sy.HasError;
        public string ErrorMessage => _err ? _errMsg : _sy.ErrorMessage;
        public long ErrorPosition => _sy.ErrorPosition;

        private void Fail(string msg)
        {
            _err = true;
            _errMsg = msg;
            Quit();
        }

        private void Quit()
        {
            _quit = true;
        }

        private void BeginScope()
        {
            ++_scopeTrack;
        }

        private bool EndScope()
        {
            return --_scopeTrack < 0;
        }

        public long ByteCount => _ir.Bytes.Count;
        public void CopyTo(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, _ir.Bytes.Count, nameof(buffer));

            _ir.Bytes.CopyTo(buffer, 0);
        }
        public void CopyTo(Span<byte> buffer)
        {
            CollectionsMarshal.AsSpan(_ir.Bytes).CopyTo(buffer);
        }

        private void Dispose(bool manual)
        {
            if (!_dispose)
            {
                _dispose = true;

                if (manual)
                {
                    _sy.Dispose();
                    _ir.Bytes.Clear();
                    _reg.Dispose();
                }
            }
        }
        ~CgContext()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static int ResultStackIsImmediate(IterativeStack<ExprItem> stack)
        {
            if (stack.Count == 1 && stack[0].IsLiteral())
            {
                ExprItem e = stack[0];
                return e.Data.Literal switch
                {
                    LiteralType.Number => 16,
                    LiteralType.String => 0,
                    LiteralType.True => 1,
                    LiteralType.False => 1,
                    LiteralType.Null => 1,
                    _ => 0,
                };
            }

            return 0;
        }

        private bool ResultStackToImmediate(StreamReader r, IterativeStack<ExprItem> stack, Span<byte> imm, out NumberLiteralType type)
        {
            ExprItem e = stack[0];
            type = NumberLiteralType.Unknown;

            switch (e.Data.Literal)
            {
                case LiteralType.Number:
                    {
                        Span<char> buf = stackalloc char[e.Length];
                        r.Goto(e);
                        r.Read(buf);
                        if (!Compiler.GetNumberLiteralInfo(buf, imm, out type, out _))
                        {
                            Fail("Unable to parse number literal.");
                            return false;
                        }

                        // Truncate size to smallest possible.
                        if ((type & NumberLiteralType.PrecisionMask) == 0)
                        {
                            int firstZero = imm.IndexOf((byte)0);
                            if (firstZero == -1)
                                firstZero = 16;
                            type |= (32 - BitOperations.TrailingZeroCount(firstZero)) switch { 
                                0 => NumberLiteralType.Bit8,
                                1 => NumberLiteralType.Bit16,
                                2 => NumberLiteralType.Bit32,
                                3 => NumberLiteralType.Bit64,
                                4 => NumberLiteralType.Bit128,
                                _ => NumberLiteralType.Bit128,
                            };
                        }
                    }
                    break;
                case LiteralType.True:
                    imm[0] = 1;
                    break;
                case LiteralType.False:
                case LiteralType.Null:
                    imm[0] = 0;
                    break;
                default:
                    Fail("Invalid result, must be an immediate value.");
                    return false;
            }

            return true;
        }

        private IRModule currentModule;
        private FunctionLayout fnMeta;
        private State nextState;

        private enum State
        {
            None,
            VarStatement,
            ReturnStatement,
            IfStatement,
        }

        public bool Process(StreamReader r, IRModule module, FunctionLayout fn, Span<Token> tokens)
        {
            _err = false;
            _errMsg = null;
            _quit = false;

            currentModule = module;
            fnMeta = fn;
            _scopeTrack = 0;
            nextState = State.None;

            State state = State.None;

            Span<Token>.Enumerator etokens = tokens.GetEnumerator();
            while (!_quit && !_sy.HasError && etokens.MoveNext())
            {
                switch (state)
                {
                    case State.None:
                        ProcessNone(ref etokens);
                        break;

                    case State.ReturnStatement:
                        ProcessReturnStatement(r, ref etokens);
                        break;

                    case State.IfStatement:
                        ProcessIfStatement(r, ref etokens);
                        break;
                }

                state = nextState;
            }

            if (state != State.None && !HasError)
                Fail($"Exiting body with invalid compiler state: {state} (next: {nextState})");

            fnMeta = default;
            currentModule = null;
            nextState = State.None;

            return !HasError;
        }

        private void ProcessNone(ref Span<Token>.Enumerator tokens)
        {
            switch (tokens.Current.Type)
            {
                case TokenType.Keyword:
                    switch (tokens.Current.Data.Keyword)
                    {
                        case KeywordType.Return:
                            nextState = State.ReturnStatement;
                            break;
                        case KeywordType.If:
                            nextState = State.IfStatement;
                            break;
                    }
                    break;

                case TokenType.Literal:
                    Fail("Literal must be part of an expression.");
                    break;
                case TokenType.Identifier:

                    break;

                case TokenType.Separator:
                    if (tokens.Current.Data.Separator != SeparatorType.Semicolon)
                    {
                        if (tokens.Current.Data.Separator == SeparatorType.CloseCurly)
                        {
                            if (EndScope())
                                Quit();
                        } else
                            Fail("Invalid separator token.");
                    }
                    break;
            }
        }

        private void ProcessReturnStatement(StreamReader r, ref Span<Token>.Enumerator tokens)
        {
            if (_sy.Process(r, currentModule, _reg, ref tokens))
            {
                if (_sy.ResultStack.Count > 0)
                {
                    int immSize = ResultStackIsImmediate(_sy.ResultStack);
                    if (immSize > 0)
                    {
                        Span<byte> imm = stackalloc byte[immSize];
                        if (!ResultStackToImmediate(r, _sy.ResultStack, imm, out NumberLiteralType numType))
                            return;

                        if (numType != NumberLiteralType.Unknown)
                            _ir.streti(imm[..numType.GetByteSize()]);
                        else
                            _ir.streti(imm);
                    } else
                    {

                    }
                }
                _ir.ret();
            }

            nextState = State.None;
        }

        private void ProcessIfStatement(StreamReader r, ref Span<Token>.Enumerator tokens)
        {
            if (!tokens.Current.IsSeparator(SeparatorType.OpenParen))
            {
                Fail("Conditional if expression must be contained within parentheses.");
                return;
            }

            if (_sy.Process(r, currentModule, _reg, ref tokens, -1, false))
            {
                _ir.bni(0, 0, [0]);
            } else
            {
                return;
            }

            if (!tokens.GetNext(out Token t) || !t.IsSeparator(SeparatorType.OpenCurly))
            {
                Fail("If branch must be contained within curly brackets.");
                return;
            }

            BeginScope();

            nextState = State.None;
        }
    }
}
