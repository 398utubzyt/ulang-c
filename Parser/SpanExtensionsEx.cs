using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ulang
{
    public static class SpanExtensionsEx
    {
        public static bool Obeys<T>(this Span<T> self, Predicate<T> predicate)
        {
            for (int i = 0; i < self.Length; ++i)
                if (!predicate(self[i]))
                    return false;
            return true;
        }

        public static bool Obeys<T>(this ReadOnlySpan<T> self, Predicate<T> predicate)
        {
            for (int i = 0; i < self.Length; ++i)
                if (!predicate(self[i]))
                    return false;
            return true;
        }

        public static void Replace<T>(this Span<T> self, ReadOnlySpan<T> from, ReadOnlySpan<T> to)
        {
            if (from.Length != to.Length)
                throw new ArgumentException("`from` and `to` must have equal lengths.");

            for (int i = 0; i < self.Length - from.Length; ++i)
                if (self.SequenceEqual(from, null))
                    to.CopyTo(self[i..]);
        }

        public static int LastIndexOfAny(this Span<char> self, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, out int which)
        {
            if (value0.Length != 0)
            {
                for (int i = 0; i <= self.Length - value0.Length; ++i)
                    if (self[i..(i + value0.Length)].SequenceEqual(value0))
                    {
                        which = 0;
                        return i;
                    }
            }

            if (value1.Length != 0)
            {
                for (int i = 0; i <= self.Length - value1.Length; ++i)
                    if (self[i..(i + value1.Length)].SequenceEqual(value1))
                    {
                        which = 1;
                        return i;
                    }
            }

            which = -1;
            return -1;
        }

        public static int LastIndexOfAny(this Span<char> self, ReadOnlySpan<char> value0, ReadOnlySpan<char> value1, ReadOnlySpan<char> value2, out int which)
        {
            if (value0.Length != 0)
            {
                for (int i = 0; i <= self.Length - value0.Length; ++i)
                    if (self[i..(i + value0.Length)].SequenceEqual(value0))
                    {
                        which = 0;
                        return i;
                    }
            }

            if (value1.Length != 0)
            {
                for (int i = 0; i <= self.Length - value1.Length; ++i)
                    if (self[i..(i + value1.Length)].SequenceEqual(value1))
                    {
                        which = 1;
                        return i;
                    }
            }

            if (value2.Length != 0)
            {
                for (int i = 0; i <= self.Length - value2.Length; ++i)
                    if (self[i..(i + value2.Length)].SequenceEqual(value2))
                    {
                        which = 2;
                        return i;
                    }
            }

            which = -1;
            return -1;
        }

        public static int LastIndexOfAny(this Span<char> self, ReadOnlySpan<string> values, out int which)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                string s = values[i];
                if (s.Length != 0)
                {
                    for (int j = 0; j <= self.Length - s.Length; ++j)
                        if (self[j..(j + s.Length)].SequenceEqual(s))
                        {
                            which = i;
                            return j;
                        }
                }
            }

            which = -1;
            return -1;
        }
    }
}
