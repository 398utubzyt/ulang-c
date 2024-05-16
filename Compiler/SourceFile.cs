using System;
using System.IO;

namespace Ulang
{
    public struct SourceFile
    {
        public string Path;
        public int Id;
        public Token[] Tokens;

        public readonly int NextOf(TokenType type, int i = 0)
        {
            for (; i < Tokens.Length; ++i)
                if (Tokens[i].Type == type)
                    return i;
            return -1;
        }
        public readonly int NextKeyword(KeywordType key, int i = 0)
        {
            for (; i < Tokens.Length; ++i)
                if (Tokens[i].Data == (long)key)
                    return i;
            return -1;
        }
        public readonly int NextOperator(OperatorType op, int i = 0)
        {
            for (; i < Tokens.Length; ++i)
                if (Tokens[i].Data == (long)op)
                    return i;
            return -1;
        }
        public readonly int NextSeparator(SeparatorType op, int i = 0)
        {
            for (; i < Tokens.Length; ++i)
                if (Tokens[i].Data == (long)op)
                    return i;
            return -1;
        }
        public readonly int NextLiteral(LiteralType op, int i = 0)
        {
            for (; i < Tokens.Length; ++i)
                if (Tokens[i].Data == (long)op)
                    return i;
            return -1;
        }

        public static unsafe bool Read(string path, out SourceFile src)
        {
            src = new SourceFile { Path = path };
            if (!File.Exists(path) || !path.EndsWith(".u"))
                return false;

            using FileStream fs = File.OpenRead(path);
            using StreamReader r = new StreamReader(fs);

            src.Tokens = Lexer.Tokenize(r);

            r.BaseStream.Seek(0, SeekOrigin.Begin);
            return src.Tokens != null;
        }
    }
}
