using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ulang
{
    internal static class Program
    {
        private static Token[] _tokenCache;
        private static string _pathCache;

        [UnmanagedCallersOnly(EntryPoint = "tokenize")]
        private static unsafe nint GetTokens(sbyte* p, Token* tokens)
        {
            string path = new string(p);

            if (tokens == null)
            {
                if (!File.Exists(path))
                    return -1;

                using FileStream fs = File.OpenRead(path);
                using StreamReader r = new StreamReader(fs);

                _pathCache = path;
                _tokenCache = Lexer.Tokenize(r);
                return _tokenCache.Length;
            }

            if (path != _pathCache)
                return -1;

            int i = 0;
            while (i < _tokenCache.Length)
                tokens[i] = _tokenCache[i++];
            _pathCache = null;
            _tokenCache = null;

            GC.Collect();

            return i;
        }

        private const string TestName = "";
        private static void Main(string[] args)
        {
            string path = 
                Path.Combine(Environment.CurrentDirectory, "../../../tests/", 
                string.IsNullOrEmpty(TestName) ? "test.u" : TestName);

            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot locate test at '{path}'.");
                return;
            }

            using FileStream fs = File.OpenRead(path);
            using StreamReader r = new StreamReader(fs);

            Token[] tokens = Lexer.Tokenize(r);
            if (tokens == null)
                return;

            Console.WriteLine("TOKENS:");
            for (int i = 0; i < tokens.Length; i++)
                Console.WriteLine(tokens[i]);
        }
    }
}