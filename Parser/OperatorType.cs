using System;

namespace Ulang
{
    public enum OperatorType
    {
        Unknown,

        Plus,
        Minus,
        Star,
        Slash,

        BitLeft,
        BitRight,
        BitAnd,
        BitOr,
        BitXor,
        BitNot,

        And,
        Or,
        Xor,

        Increment,
        Decrement,

        PlusEqual,
        MinusEqual,
        StarEqual,
        SlashEqual,

        BitLeftEqual,
        BitRightEqual,
        BitAndEqual,
        BitOrEqual,
        BitXorEqual,
        BitNotEqual,

        AndEqual,
        OrEqual,
        XorEqual,

        Less,
        Greater,
        Not,
        Equal,
        LessEqual,
        GreaterEqual,
        NotEqual,
        EqualEqual,

        Option,
        OptionElse,
        OptionEqual,
    }
}
