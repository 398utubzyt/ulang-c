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
    }
}
