using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulang
{
    public static class Errors
    {
        public struct Error(int code, string format)
        {
            public int Code = code;
            public string Format = format;
        }

        public static void Print(Error err)
            => Ulang.Compiler.PrintError($"An error occurred: (U{err.Code})\n{err.Format}");
        public static void Print(Error err, object arg0)
            => Ulang.Compiler.PrintError($"An error occurred: (U{err.Code})\n{string.Format(err.Format, arg0)}");
        public static void Print(Error err, object arg0, object arg1)
            => Ulang.Compiler.PrintError($"An error occurred: (U{err.Code})\n{string.Format(err.Format, arg0, arg1)}");
        public static void Print(Error err, object arg0, object arg1, object arg2)
            => Ulang.Compiler.PrintError($"An error occurred: (U{err.Code})\n{string.Format(err.Format, arg0, arg1, arg2)}");
        public static void Print(Error err, params object[] args)
            => Ulang.Compiler.PrintError($"An error occurred: (U{err.Code})\n{string.Format(err.Format, args)}");

        public static readonly Error Unknown = new(0001, "Unknown error.");
        public static readonly Error ExpressionCannotBeStatement = new(0200, "Expression cannot be used as a statement.");
        public static readonly Error VariableIsNotExpression = new(0200, "Identifier cannot be used as an expression.");
        public static readonly Error IdentifierIsNotVariable = new(0400, "Identifier is not a variable identifier.");
    }
}
