using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Ulang
{
    public static class Lexer
    {
        private static readonly Regex CommentRegex = new Regex("(?:\\/\\*[^\\*]*\\*\\/)|(?:\\/\\/[^\\n]*)", RegexOptions.Multiline);
        private static readonly EnumDB<KeywordType> _keywords =
            new("public",
                "private",
                "protected",
                "internal",
                "new",
                "drop",
                "if",
                "else",
                "match",
                "while",
                "return",
                "continue",
                "break",
                "struct",
                "interface",
                "union",
                "enum",
                "impl",
                "namespace",
                "using",
                "extern",
                "const",
                "compt");
        private static readonly EnumDB<LiteralType> _literals =
            new("true",
                "false",
                "null");
        private static readonly EnumCharDB<SeparatorType> _seperators =
            new('{', '}', '[', ']', '(', ')', ';', ':', ',', '.');
        private static readonly EnumDB<OperatorType> _operators =
            new("+",
                "-",
                "*",
                "/",
                "<<",
                ">>",
                "&",
                "|",
                "^",
                "~",
                "&&",
                "||",
                "^^",
                "++",
                "--",
                "+=",
                "-=",
                "*=",
                "/=",
                "<<=",
                ">>=",
                "&=",
                "|=",
                "^=",
                "~=",
                "&&=",
                "||=",
                "^^=",
                "<",
                ">",
                "!",
                "=",
                "<=",
                ">=",
                "!=",
                "==",
                "?",
                "??",
                "??=");

        public static int SizeOf(LiteralType t)
            => _literals.Get(t).Length;
        public static int SizeOf(KeywordType t)
            => _keywords.Get(t).Length;
        public static int SizeOf(OperatorType t)
            => _operators.Get(t).Length;

        private static void ClearAllLineComments(Span<char> chars)
        {
            int cur;
            while ((cur = chars.IndexOf("//")) >= 0)
            {
                while (cur < chars.Length && chars[cur] != '\n')
                    chars[cur++] = '\0';
            }
        }

        private static bool ReadUntilNext(StreamReader r, Span<char> chars, ref long start)
        {
            if (chars.Contains('\0'))
                start += chars.IndexOf('\0');
            if (start > 0)
                start++;

            int c = r.Read(), p = 0;

            while (c >= 0 && char.IsWhiteSpace((char)c))
            {
                c = r.Read();
                ++start;
            }

            while (c >= 0)
            {
                if (char.IsWhiteSpace((char)c))
                {
                    chars[p] = '\0';
                    return true;
                }

                chars[p++] = (char)c;
                c = r.Read();
            }

            chars[p] = '\0';
            ClearAllLineComments(chars);

            return p > 0;
        }

        private static bool SpanPass<T>(Span<T> span, Func<T, bool> func, Func<T, bool> cont)
        {
            for (int i = 0; i < span.Length && cont(span[i]); i++)
                if (!func(span[i]))
                    return false;
            return true;
        }

        private static bool IsNotNullChar(char c)
            => c != 0;
        private static bool IsValidIdentifierChar(char c)
            => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '@';
        private static bool IsValidNumberPrefix(char c)
            => c == 'x' || c == 'b';

        private static bool AnalyzeNoIdentifier(StreamReader r, Span<char> buffer, long pos, List<Token> tokens)
        {
            if (_keywords.Has(buffer, out KeywordType word))
            {
                tokens.Add(Token.MakeKeyword(r, pos, word));
                return true;
            }

            if (_operators.Has(buffer, out OperatorType op))
            {
                tokens.Add(Token.MakeOperator(r, pos, op));
                // for (int i = SizeOf(op); i < buffer.Length && _operators.Has(buffer[i..], out OperatorType op2); i += SizeOf(op2))
                //     tokens.Add(Token.MakeOperator(r, pos += SizeOf(op2), op2));
                return true;
            }

            if (buffer.Length == 1 && _seperators.Has(buffer[0], out SeparatorType sep))
            {
                tokens.Add(Token.MakeSeparator(r, pos, sep));
                //for (int i = 2; i <= buffer.Length && _seperators.Has(buffer[^i], out SeperatorType sep2); ++i)
                //    tokens.Add(Token.MakeSeperator(r, ++pos, sep2));
                return true;
            }

            if (_literals.Has(buffer, out LiteralType lit))
            {
                tokens.Add(Token.MakeLiteral(r, pos, lit));
                return true;
            }

            return false;
        }

        private static bool AnalyzeMoreLiterals(StreamReader r, Span<char> buffer, long pos, List<Token> tokens)
        {
            if (double.TryParse(buffer, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out _) ||
                (buffer.StartsWith("0x") && Int128.TryParse(buffer[..2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out _)) ||
                (buffer.StartsWith("0b") && Int128.TryParse(buffer[..2], NumberStyles.AllowBinarySpecifier, CultureInfo.InvariantCulture, out _)))
            {
                tokens.Add(Token.MakeLiteral(r, pos, buffer.Length, LiteralType.Number));
                return true;
            }

            if (buffer.Count('\"') == 2 && buffer[0] == '\"' && buffer[^1] == '\"')
            {
                tokens.Add(Token.MakeLiteral(r, pos, LiteralType.String));
                return true;
            }

            return false;
        }

        private static bool AnalyzeIdentifier(StreamReader r, Span<char> buffer, long pos, List<Token> tokens)
        {
            if (SpanPass(buffer, IsValidIdentifierChar, IsNotNullChar) && !char.IsAsciiDigit(buffer[0]))
            {
                int len = buffer.Contains('\0') ? buffer.IndexOf('\0') : buffer.Length;
                tokens.Add(Token.MakeIdentifier(r, pos, len));
                return true;
            }

            return false;
        }

        private static bool Analyze(StreamReader r, Span<char> buffer, ref long pos, List<Token> tokens)
            => AnalyzeNoIdentifier(r, buffer, pos, tokens) || AnalyzeIdentifier(r, buffer, pos, tokens) || AnalyzeMoreLiterals(r, buffer, pos, tokens);

        private static bool NoWhitespace(StreamReader r, Span<char> buffer, List<Token> tokens, ref long pos)
        {
            // Parse buffer somehow
            // Cannot rely on whitespace here...
            int len = buffer.IndexOf('\0');
            if (len < 0)
                len = buffer.Length;

            if (len == 0)
                return true;

            int p = 0;
            while (p < len)
            {
                if (AnalyzeNoIdentifier(r, buffer[p..len], pos + p, tokens))
                    goto SuccessfulFind;
                ++p;
            }

            // Prioritize non-identifiers over identifiers
            p = 0;
            while (p < len)
            {
                if (AnalyzeIdentifier(r, buffer[p..len], pos + p, tokens))
                    goto SuccessfulFind;
                ++p;
            }

            p = 0;
            while (p < len)
            {
                if (AnalyzeMoreLiterals(r, buffer[p..len], pos + p, tokens))
                    goto SuccessfulFind;
                ++p;
            }

            Console.WriteLine($"Could not find identifier somehow in `{buffer[..len]}`.");
            return false;

        SuccessfulFind:
            if (p != len && !NoWhitespace(r, buffer[..p], tokens, ref pos))
            {
                Console.WriteLine("No whitespace subsearch failed.");
                return false;
            }

            return true;
        }

        public static Token[] Tokenize(StreamReader r)
        {
            List<Token> tokens = new List<Token>();
            Span<char> buffer = stackalloc char[1024]; // Might be excessive
            long pos = 0, start = 0;

            while (ReadUntilNext(r, buffer, ref start))
            {
                pos = start;
                if (Analyze(r, buffer, ref pos, tokens))
                    continue;

                List<Token> temptokens = new List<Token>(2);
                if (!NoWhitespace(r, buffer, temptokens, ref pos))
                    return null;
                temptokens.Reverse();
                tokens.AddRange(temptokens);
            }

            return tokens.ToArray();
        }
    }
}