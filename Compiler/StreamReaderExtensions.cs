using System;
using System.IO;
using System.Text;

namespace Ulang
{
    public static class StreamReaderExtensions
    {
        public static long Goto(this StreamReader reader, Token[] tokens, int index = 0)
            => Goto(reader, tokens[index]);
        public static long Goto(this StreamReader reader, Span<Token> tokens, int index = 0)
            => Goto(reader, tokens[index]);
        public static long Goto(this StreamReader reader, ReadOnlySpan<Token> tokens, int index = 0)
            => Goto(reader, tokens[index]);
        public static long Goto(this StreamReader reader, Token token)
        {
            reader.DiscardBufferedData();
            return reader.BaseStream.Seek(token.Position, SeekOrigin.Begin);
        }
        public static long Goto(this StreamReader reader, ExprItem item)
        {
            reader.DiscardBufferedData();
            return reader.BaseStream.Seek(item.Position, SeekOrigin.Begin);
        }

        public static int Read(this StreamReader reader, Span<char> span, int max)
        {
            return reader.Read(span[..max]);
        }

        public static nint ReadLine(this StreamReader reader, Span<char> span)
        {
            if (reader.EndOfStream)
                return -1;

            if (span.Length == 0)
                return 0;

            int i = 0;
            char ch;
            while ((ch = (char)reader.Read()) >= 0)
            {
                if (i + 1 >= span.Length || ch == '\r' || ch == '\n')
                {
                    if (ch == '\r' && reader.Peek() == '\n')
                        reader.Read();

                    goto Terminate;
                }

                span[i++] = ch;
            }

            return -1;

        Terminate:
            span[i] = '\0';
            return i;
        }
    }
}
