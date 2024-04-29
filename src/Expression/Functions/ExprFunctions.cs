namespace Derivate;

public partial class Func
{
    /// <summary>
    /// Returns the base of the given function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static IExpression Base(this IExpression function)
    {
        return function switch
        {
            Number or Fraction => Undefined,
            Power f => f.Base,
            _ => function
        };
    }

    /// <summary>
    /// Returns the exponent part of the given function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static IExpression Exponent(this IExpression function)
    {
        return function switch
        {
            Number or Fraction => Undefined,
            Power f => f.Exponent,
            _ => Num(1)
        };
    }

    /// <summary>
    /// Returns the term part of the function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static IExpression Term(this IExpression function)
    {
        return function switch
        {
            Number or Fraction
                => Undefined,
            Product u when u.value.First() is Number or Fraction
                => Mul(u.value.Skip(1)),
            Product u
                => u,
            _ => Mul(function)
        };
    }

    /// <summary>
    /// Returns the constant part of the function or the Symbol Undefined
    /// </summary>
    /// <param name="function">The given simplified function</param>
    public static IExpression Const(this IExpression function)
    {
        return function switch
        {
            Number or Fraction
                => Undefined,
            Product u when u.value.First() is Number or Fraction
                => u.value.First(),
            Product
                => Num(1),
            _ => Num(1),
        };
    }
    
    /// <summary>Returns false when <paramref name="other"/> is equal to a 
    /// complete sub-expression of <paramref name="expr"/> and true if
    /// otherwise</summary>
    public static bool FreeOf(this IExpression expr, IExpression other)
    {
        if (expr.Equals(other))
            return false;
        if (expr is Symbols or Constant)
            return true;

        foreach (var operand in expr.ToList())
        {
            if (!operand.FreeOf(other))
                return false;  
        }

        return true;
    }

    /// <summary>Checks if expr is free of all expressions in the set 
    /// <paramref name="other"/></summary>
    public static bool FreeOf(this IExpression expr, HashSet<IExpression> other)
    {
        foreach (var element in other)
        {            
            foreach (var operand in expr.ToList())
            {
                if (!operand.FreeOf(element))
                    return false;
            }
        }

        return true;
    }

    /// <summary>Returns the list of operands of the expression</summary>
    public static List<IExpression> ToList(this IExpression expr)
    {
        return expr switch
        {
            Sum a => a.value,
            Product a => a.value,
            Power a => [a.Base, a.Exponent],
            _ => [expr],
        };
    }
    
    public static bool CompareList(List<IExpression> m, List<IExpression> n)
    {
        int minCount = Math.Min(m.Count, n.Count);

        for (int i = 1; i <= minCount; i++)
        {
            int mi = m.Count - i, ni = n.Count - i;
            if (!m[mi].Equals(n[ni]))
                return m[mi].CompareTo(n[ni]);
        }
        return m.Count < n.Count;
    }
}