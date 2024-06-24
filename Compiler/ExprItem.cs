using System.Runtime.InteropServices;

namespace Ulang
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ExprData
    {
        [FieldOffset(0)]
        public LiteralType Literal;
        [FieldOffset(0)]
        public OperatorType Operator;
        [FieldOffset(0)]
        public KeywordType Keyword;
        [FieldOffset(0)]
        public uint Id;
    }

    public struct ExprItem
    {
        public long Position;
        public int Length;
        public ExprType Type;
        public ExprData Data;

        public readonly bool IsUndefined() => Type == ExprType.Unknown;
        public readonly bool IsLiteral() => Type == ExprType.Literal;
        public readonly bool IsLiteral(LiteralType type) => Type == ExprType.Literal && Data.Literal == type;
        public readonly bool IsOperator() => Type == ExprType.UnaryOperator || Type == ExprType.BinaryOperator;
        public readonly bool IsUnaryOperator() => Type == ExprType.UnaryOperator;
        public readonly bool IsUnaryOperator(OperatorType type) => Type == ExprType.UnaryOperator && Data.Operator == type;
        public readonly bool IsBinaryOperator() => Type == ExprType.BinaryOperator;
        public readonly bool IsBinaryOperator(OperatorType type) => Type == ExprType.BinaryOperator && Data.Operator == type;
        public readonly bool IsLocal(uint id) => Type == ExprType.Local && Data.Id == id;
        public readonly bool IsGlobal(uint id) => Type == ExprType.Global && Data.Id == id;
        public readonly bool IsField(uint id) => Type == ExprType.Field && Data.Id == id;
        public readonly bool IsVar() => Type >= ExprType.Local && Type <= ExprType.Field;
        public readonly bool IsFunctionCall(uint id) => Type == ExprType.FuncCall && Data.Id == id;
        public readonly bool IsFunctionParam(uint id) => Type == ExprType.FuncParam && Data.Id == id;
        public readonly bool IsTypeRef(uint id) => Type == ExprType.Type && Data.Id == id;
        public readonly bool IsScopeBegin() => Type == ExprType.ScopeBegin;
        public readonly bool IsScopeEnd() => Type == ExprType.ScopeEnd;
        public readonly bool IsAccessor() => Type == ExprType.Accessor;

        public static ExprItem Literal(Token token, LiteralType type) => new(token.Position, token.Length, ExprType.Literal, type);
        public static ExprItem UnaryOperator(Token token, OperatorType type) => new ExprItem(token.Position, ExprType.UnaryOperator, type);
        public static ExprItem BinaryOperator(Token token, OperatorType type) => new ExprItem(token.Position, ExprType.BinaryOperator, type);
        public static ExprItem Local(Token token, uint id) => new ExprItem(token.Position, ExprType.Local, id);
        public static ExprItem Global(Token token, uint id) => new ExprItem(token.Position, ExprType.Global, id);
        public static ExprItem Field(Token token, uint id) => new ExprItem(token.Position, ExprType.Field, id);
        public static ExprItem FunctionCall(Token token, uint id) => new ExprItem(token.Position, ExprType.FuncCall, id);
        public static ExprItem FunctionParam(Token token, uint id) => new ExprItem(token.Position, ExprType.FuncParam, id);
        public static ExprItem TypeRef(Token token, uint id) => new ExprItem(token.Position, ExprType.Type, id);
        public static ExprItem ScopeBegin(Token token) => new ExprItem(token.Position, ExprType.ScopeBegin);
        public static ExprItem ScopeEnd(Token token) => new ExprItem(token.Position, ExprType.ScopeEnd);
        public static ExprItem Accessor(Token token) => new ExprItem(token.Position, ExprType.Accessor);

        public ExprItem(long pos, ExprType type, ExprData data)
        {
            Position = pos;
            Type = type;
            Data = data;
        }

        public ExprItem(long pos, ExprType type)
        {
            Position = pos;
            Type = type;
        }

        public ExprItem(long pos, int len, ExprType type, LiteralType lit)
        {
            Position = pos;
            Length = len;
            Type = type;
            Data.Literal = lit;
        }

        public ExprItem(long pos, ExprType type, OperatorType op)
        {
            Position = pos;
            Type = type;
            Data.Operator = op;
        }

        public ExprItem(long pos, ExprType type, KeywordType key)
        {
            Position = pos;
            Type = type;
            Data.Keyword = key;
        }

        public ExprItem(long pos, ExprType type, uint id)
        {
            Position = pos;
            Type = type;
            Data.Id = id;
        }
    }
}
