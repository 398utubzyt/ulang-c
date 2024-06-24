namespace Ulang
{
    public enum ExprType
    {
        Unknown,
        Literal,
        UnaryOperator,
        BinaryOperator,
        Local,
        Global,
        Field,
        FuncCall,
        FuncParam,
        Type,
        ScopeBegin,
        ScopeEnd,
        Accessor,
        Statement,
        Flow,
    }
}
