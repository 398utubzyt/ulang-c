using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ulang.Layouts;

namespace Ulang
{
    public sealed class SyContext : IDisposable
    {
        private static readonly string DefaultErrorMessage = "Completed successfully.";

        private bool _disposed;

        private string _errMsg;
        private long _errPos;
        private bool _err;
        private bool _quit;

        private bool isCompt;

        private int scopeTrack;
        private bool isLiteral;
        private Token prevToken;
        private ExprItem prevItem;

        private readonly IterativeStack<ExprItem> _working = new IterativeStack<ExprItem>();
        private readonly IterativeStack<ExprItem> _output = new IterativeStack<ExprItem>();
        private readonly IterativeStack<ExprItem> _result = new IterativeStack<ExprItem>();

        private class FieldDesc
        {
            public string Name;
            public Dictionary<string, FieldDesc> Subfields = [];
        }

        private readonly Dictionary<string, FieldDesc> _fields = [];

        public IterativeStack<ExprItem> ResultStack => _result;
        public bool IsLiteral => isLiteral;
        public bool HasError => _err;
        public string ErrorMessage => _errMsg;
        public long ErrorPosition => _errPos;

        public enum ResultType
        {
            Invalid,
            Immediate,
            Field,
            Global,
            Local,
            Complex // Multiple types in one result.
        }

        public ResultType GetTypeOfResult()
        {
            if (_err || _result.Count == 0)
                return ResultType.Invalid;

            else if (_result.Count == 1)
            {
                return _result[0].Type switch
                {
                    ExprType.Literal => ResultType.Immediate,
                    ExprType.Field => ResultType.Field,
                    ExprType.Global => ResultType.Global,
                    ExprType.Local => ResultType.Local,
                    _ => ResultType.Invalid,
                };
            }

            // Syntax checks.

            else
            {
                switch (_result[0].Type)
                {
                    case ExprType.UnaryOperator:
                    case ExprType.BinaryOperator:
                    case ExprType.Accessor:
                        return ResultType.Invalid;
                }

                if (_result[1].IsBinaryOperator())
                    return ResultType.Invalid;

                for (int i = 1; i < _result.Count; ++i)
                {
                    if (_result[i].IsAccessor() && _result[i - 1].IsVar())
                        ;
                }

                return ResultType.Invalid;
            }
        }

        public bool IsCompt { get => isCompt; set => isCompt = value; }

        private void Fail(Token token, string msg)
        {
            _err = true;
            _errMsg = msg;
            _errPos = token.Position;
        }

        private void Fail(ExprItem item, string msg)
        {
            _err = true;
            _errMsg = msg;
            _errPos = item.Position;
            Quit();
        }

        private void Quit()
        {
            _quit = true;
        }

        private void PushOutput(ExprItem item)
        {
            prevItem = item;
            _output.PushBack(item);
        }
        private void PushWork(ExprItem item)
        {
            prevItem = item;
            _working.PushBack(item);
        }
        private void PopWorkToOutput()
        {
            if (_working.TryPopBack(out ExprItem item))
                _output.PushBack(item);
        }

        public bool Process(StreamReader r, IRModule module, RegisterSim registers,
            ref Span<Token>.Enumerator tokens, int initialScope = 0, bool failOnBadScope = true)
        {
            scopeTrack = initialScope;
            isLiteral = true;
            _err = false;
            _errMsg = DefaultErrorMessage;

            do
            {
                Token t = tokens.Current;
                switch (t.Type)
                {
                    case TokenType.Identifier:
                        ProcessIdentifier(r, module, registers, t);
                        break;

                    case TokenType.Keyword:
                        break;

                    case TokenType.Separator:
                        if (tokens.Current.Data.Separator == SeparatorType.Semicolon)
                        {
                            goto QuitLoop;
                        }

                        ProcessSeparator(t);
                        break;

                    case TokenType.Operator:
                        ProcessOperator(t);
                        break;

                    case TokenType.Literal:
                        ProcessLiteral(t);
                        break;
                }
                prevToken = t;
            } while (!_quit && tokens.MoveNext());

        QuitLoop:

            if (failOnBadScope && scopeTrack != 0)
                Fail(prevToken, $"Scope when exiting expression is not correct. ({scopeTrack})");

            SolverPass(r);

            return !_err;
        }

        private void SolverPass(StreamReader r)
        {
            while (_output.TryPopFront(out ExprItem item))
            {
                switch (item.Type)
                {
                    case ExprType.Literal:
                    case ExprType.Field:
                    case ExprType.Local:
                    case ExprType.Global:
                    case ExprType.Accessor:
                        _result.PushBack(item);
                        break;

                    case ExprType.UnaryOperator:
                        {
                            if (_result.TryPeek(out ExprItem arg))
                            {
                                if (arg.IsLiteral(LiteralType.Number))
                                    SolveUnaryOpForNumberLiteral(r, item, arg);
                                else
                                    _result.PushBack(arg);
                            } else
                                Fail(item, "Unary operator requires 1 argument.");
                        } break;
                    case ExprType.BinaryOperator:
                        {
                            _result.PushBack(item);
                        } break;
                }
            }
        }

        private void SolveUnaryOpForNumberLiteral(StreamReader r, ExprItem op, ExprItem arg0)
        {
            Span<char> tempBuffer = stackalloc char[arg0.Length];
            r.Goto(arg0);
            r.Read(tempBuffer);
            Span<byte> tempNum = stackalloc byte[16]; // Max precision is 128-bits at the moment
            if (!Compiler.GetNumberLiteralInfo(tempBuffer, tempNum, out NumberLiteralType numType, out NumberLiteralFormat numFmt))
            {
                Fail(arg0, "Failed to parse number literal.");
            }
            Span<byte> actualNum = tempNum[..((numType & NumberLiteralType.PrecisionMask) switch
            {
                NumberLiteralType.Bit8 => 1,
                NumberLiteralType.Bit16 => 2,
                NumberLiteralType.Bit32 => 4,
                NumberLiteralType.Bit64 => 8,
                NumberLiteralType.Bit128 => 16,
                _ => 16,
            })];

            switch (op.Data.Operator)
            {
                case OperatorType.BitNot:
                    for (int i = 0; i < actualNum.Length; ++i)
                        actualNum[i] = (byte)~actualNum[i];
                    break;
                case OperatorType.Not:
                    for (int i = 0; i < actualNum.Length; ++i)
                        if (actualNum[i] != 0)
                        {
                            actualNum.Clear();
                            break;
                        }
                    break;
                case OperatorType.Star:
                    Fail(op, "Cannot dereference a numeric literal.");
                    break;

                default:
                    Fail(op, "Invalid operator for a numeric literal.");
                    return;
            }
        }

        private void ProcessLiteral(Token token)
        {
            Console.WriteLine("Push literal");
            PushOutput(ExprItem.Literal(token, token.Data.Literal));
        }

        private void ProcessKeyword(Token token)
        {
            switch (token.Data.Keyword)
            {
                default:
                    Fail(token, "Invalid keyword in expression.");
                    break;
            }
        }

        private void ProcessSeparator(Token token)
        {
            switch (token.Data.Separator)
            {
                case SeparatorType.CloseParen:
                    {
                        using IEnumerator<ExprItem> e = _working.GetPopEnumerator();
                        ExprItem item;
                        while (e.MoveNext())
                        {
                            item = e.Current;
                            if (item.IsScopeBegin())
                                goto ScopeStartFound;
                            PushOutput(item);
                        }

                        Fail(token, "Closing parenthesis does not have a corresponding open parenthesis before it.");

                    ScopeStartFound:
                        if (scopeTrack-- == 0)
                            Quit();
                    } break;

                case SeparatorType.OpenParen:
                    PushWork(ExprItem.ScopeBegin(token));
                    ++scopeTrack;
                    break;

                //case SeparatorType.OpenCurly:
                //    PushOutput(ExprItem.ScopeBegin(token));
                //    ++scopeTrack;
                //    break;
                //case SeparatorType.CloseCurly:
                //    PushOutput(ExprItem.ScopeEnd(token));
                //    --scopeTrack;
                //    break;
                case SeparatorType.OpenCurly:
                case SeparatorType.CloseCurly:
                    Fail(token, "Curly brackets are not allowed in expressions.");
                    break;

                case SeparatorType.Dot:
                    if (prevToken.IsIdentifier())
                        PushWork(ExprItem.Accessor(token));
                    else
                        Fail(token, "Only variable properties can be accessed.");
                    break;
            }
        }

        private void ProcessOperator(Token token)
        {
            switch (token.Data.Operator)
            {
                case OperatorType.Plus:
                case OperatorType.Minus:
                case OperatorType.Star:
                    if (prevItem.IsOperator() || prevItem.IsScopeBegin() || prevItem.IsUndefined())
                    {
                        PushWork(ExprItem.UnaryOperator(token, token.Data.Operator));
                        break;
                    }
                    goto default;

                default:
                    if (prevToken.IsIdentifier() || prevToken.IsSeparator(SeparatorType.CloseParen))
                        PushWork(ExprItem.BinaryOperator(token, token.Data.Operator));
                    else
                        Fail(token, "Binary operator must follow an identifier.");
                    break;
            }
        }

        private void ProcessIdentifier(StreamReader r, IRModule module, RegisterSim registers, Token token)
        {
            
            if (prevItem.IsAccessor())
            {
                PopWorkToOutput();
            }
        }

        #region IDisposable
        private void Dispose(bool manual)
        {
            if (!_disposed)
            {
                if (manual)
                {
                }

                _disposed = true;
            }
        }

        ~SyContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
