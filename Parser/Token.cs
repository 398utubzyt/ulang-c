using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ulang
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TokenData
    {
        [FieldOffset(0)]
        public long Raw;
        [FieldOffset(0)]
        public KeywordType Keyword;
        [FieldOffset(0)]
        public OperatorType Operator;
        [FieldOffset(0)]
        public SeparatorType Separator;
        [FieldOffset(0)]
        public LiteralType Literal;

        public static implicit operator long(TokenData data) => data.Raw;
        public static implicit operator TokenData(long data) => new() { Raw = data };
        public static implicit operator KeywordType(TokenData data) => data.Keyword;
        public static implicit operator TokenData(KeywordType data) => new() { Keyword = data };
        public static implicit operator OperatorType(TokenData data) => data.Operator;
        public static implicit operator TokenData(OperatorType data) => new() { Operator = data };
        public static implicit operator SeparatorType(TokenData data) => data.Separator;
        public static implicit operator TokenData(SeparatorType data) => new() { Separator = data };
        public static implicit operator LiteralType(TokenData data) => data.Literal;
        public static implicit operator TokenData(LiteralType data) => new() { Literal = data };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Token
    {
        public long Position;
        public TokenData Data;
        public TokenType Type; // i32
        public int Length;

        public static Token MakeIdentifier(StreamReader r, long start, int length)
            => new Token { Type = TokenType.Identifier, Position = start, Length = length };
        public static Token MakeKeyword(StreamReader r, long start, KeywordType type)
            => new Token { Type = TokenType.Keyword, Position = start, Length = Lexer.SizeOf(type), Data = type };
        public static Token MakeSeparator(StreamReader r, long start, SeparatorType type)
            => new Token { Type = TokenType.Separator, Position = start, Length = 1, Data = type };
        public static Token MakeOperator(StreamReader r, long start, OperatorType type)
            => new Token { Type = TokenType.Operator, Position = start, Length = Lexer.SizeOf(type), Data = type };
        public static Token MakeLiteral(StreamReader r, long start, LiteralType type)
            => new Token { Type = TokenType.Literal, Position = start, Length = Lexer.SizeOf(type), Data = type };
        public static Token MakeLiteral(StreamReader r, long start, int length, LiteralType type)
            => new Token { Type = TokenType.Literal, Position = start, Length = length, Data = type };
        public static Token MakeComment(StreamReader r, long start, int length)
            => new Token { Type = TokenType.Comment, Position = start, Length = length };

        public readonly bool IsKeyword(KeywordType type) => Type == TokenType.Keyword && Data.Keyword == type;
        public readonly bool IsSeparator(SeparatorType type) => Type == TokenType.Separator && Data.Separator == type;
        public readonly bool IsOperator(OperatorType type) => Type == TokenType.Separator && Data.Operator == type;
        public readonly bool IsLiteral(LiteralType type) => Type == TokenType.Literal && Data.Literal == type;
        public readonly bool IsIdentifier() => Type == TokenType.Identifier;

        public override readonly string ToString()
        {
            return $"{Type} ({Position}-{Position + Length}): {
                Type switch 
                { 
                    TokenType.Identifier => "IDENTIFIER",
                    TokenType.Keyword => Data.Keyword.ToString(),
                    TokenType.Separator => Data.Separator.ToString(),
                    TokenType.Operator => Data.Operator.ToString(),
                    TokenType.Literal => Data.Literal.ToString(),
                    TokenType.Comment => "COMMENT",
                    _ => Data.ToString(),
                }}";
        }

        public readonly string ToString(StreamReader file)
        {
            Span<char> buffer = stackalloc char[Length];
            file.DiscardBufferedData();
            file.BaseStream.Seek(Position, SeekOrigin.Begin);
            file.Read(buffer);
            return $"{Type} ({Position}-{Position + Length}) [{Type switch
            {
                TokenType.Identifier => "IDENTIFIER",
                TokenType.Keyword => Data.Keyword.ToString(),
                TokenType.Separator => Data.Separator.ToString(),
                TokenType.Operator => Data.Operator.ToString(),
                TokenType.Literal => Data.Literal.ToString(),
                TokenType.Comment => "COMMENT",
                _ => Data.ToString(),
            }}]: `{buffer}`";
        }
    }
}
