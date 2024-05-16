using System;

namespace Ulang
{
    public sealed class EnumCharDB<T> where T : unmanaged, Enum
    {
        private readonly char[] _chars;

        public unsafe char Get(T type)
            => _chars[--*(int*)&type];
        public unsafe bool Has(char c, out T ret)
        {
            T type = ret = TypeOf(c);
            return (*(int*)&type) != 0;
        }
        public unsafe T TypeOf(char c)
        {
            int i = 0;
            while (i < _chars.Length)
                if (c == _chars[i++])
                    return *(T*)&i;
            i = 0;
            return *(T*)&i;
        }

        public EnumCharDB(params char[] chars)
            => _chars = chars;
    }
}
