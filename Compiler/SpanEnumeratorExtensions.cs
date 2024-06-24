using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public static class SpanEnumeratorExtensions
    {
        public static bool GetNext(this ref Span<Token>.Enumerator self, out Token token)
        {
            bool result = self.MoveNext();
            token = self.Current;
            return result;
        }
    }
}
