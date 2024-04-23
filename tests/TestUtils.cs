using F = Derivate.Func;
using Derivate;

namespace DerivateTests;

public static class ExpressionUtil
{
    public static IExpression Term(int lc, int exp = 0, IExpression? var = null)
    {
        var ??= F.Var("x");
        return (lc, exp) switch
        {
            (0, _) => F.Num(0),
            (_, 0) => F.Num(lc),
            (1, 1) => var,
            (1, _) => F.Pow(var, F.Num(exp)),
            (_, 1) => F.Mul(F.Num(lc), var),
            (_, _) => F.Mul(F.Num(lc), F.Pow(var, F.Num(exp))),
        };
    }
}