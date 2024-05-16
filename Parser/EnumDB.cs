using System;

namespace Ulang
{
    public sealed class EnumDB<T> where T : unmanaged, Enum
    {
        private readonly string[] _words;

        public unsafe string Get(T type)
            => _words[--*(int*)&type];
        public unsafe bool Has(Span<char> word, out T ret)
        {
            T type = ret = TypeOf(word);
            return (*(int*)&type) != 0;
        }
        public unsafe T TypeOf(Span<char> word)
        {
            int ind = word.IndexOf('\0');
            if (ind >= 0)
                word = word[..ind];
            int i = 0;
            while (i < _words.Length)
                if (word.SequenceEqual(_words[i++]))
                    return *(T*)&i;
            i = 0;
            return *(T*)&i;
        }

        public EnumDB(params string[] words)
            => _words = words;
    }
}
