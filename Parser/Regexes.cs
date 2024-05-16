using System;
using System.Text.RegularExpressions;

namespace Ulang
{
    public static class Regexes
    {
        public static readonly Regex Comment = new Regex("(?:\\/\\*[^\\*]*\\*\\/)|(?:\\/\\/[^\\n]*)", RegexOptions.Multiline);
    }
}
