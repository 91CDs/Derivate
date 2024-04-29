namespace Derivate;

public static partial class Func
{
    /// <summary> Returns the expanded form of the polynomial f assuming 
    /// f is already simplified </summary>
    /// <param name="f">A polynomial</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IExpression Expand(this IExpression f)
    {
        return f switch
        {
            Power (Sum expr, Number pow)
                => ExpandPower(expr, pow),
            Product n 
                => ExpandProduct(n.value),
            Sum n
                => ExpandSum(n.value),

            _ => f,
        };
    }

    private static IExpression ExpandSum(List<IExpression> operands)
    {
        return Add(operands.Select(Expand)).Simplify();
    }

    private static IExpression ExpandPower(Sum Base, Number exponent)
    {
        return exponent.value switch
        {
            > 0 => MultinomialExpansion(Base, exponent.value),
            < 0 => Base,
            0   => Num(1),
        };
    }

    private static IExpression ExpandProduct(List<IExpression> operands)
    {
        if (operands.Count == 1)
            return operands[0];
        
        if (operands.Count == 2)
            return (operands[0], operands[1]) switch
            {
                (var a, var b)
                when a is Sum or Power && b is Sum or Power
                    => DistributeTerms(ToSumList(a.Expand()), ToSumList(b.Expand())),
                (var a, var b)
                when b is Sum or Power
                    => DistributeTerms([a], ToSumList(b.Expand())),
                (var a, var b)
                when a is Sum or Power
                    => DistributeTerms(ToSumList(a.Expand()), [b]),

                _ => Mul(operands),
            };
        
        // Expand all operands for cases like powers inside products
        operands = operands.Select(Expand).ToList();
        IExpression rest = ExpandProduct(operands.Skip(1).ToList());
        return ExpandProduct([operands.First(), rest]).Simplify();

        List<IExpression> ToSumList(IExpression a) 
            => a is Sum sum ? sum.value : [a];
    }

    /// <summary>
    /// Calculates the algebraic expansion of a polynomial using a recursive
    /// version of the binomial theorem     <br />
    /// It is given by the formula:         <br />
    ///   Σ^{n}_{k=0} = (nCk) x^{n-k} x^{k} <br />
    ///   where n is the exponent and nCk is the binomial coefficient
    /// </summary>
    /// <param name="expr">polynomial</param>
    /// <param name="exponent">exponent of the polynomial</param>
    private static IExpression MultinomialExpansion(Sum expr, int exponent)
    {
        if (expr.value.Count == 1)
            return Pow(expr.value[0], Num(exponent));

        // Expand expression for cases like powers inside sums
        IExpression expandedSum = ExpandSum(expr.value);
        List<IExpression> expandedSumList = expandedSum is Sum sum 
            ? sum.value : [expandedSum];
        
        IExpression f = expandedSumList.First();
        Sum g = Add(expandedSumList.Skip(1));
        
        List<IExpression> sumList = [];
        for (int k = 0; k <= exponent; k++)
        {
            int coefficient = (int) MathInt.BinomialCoefficient(
                (uint) exponent, 
                (uint) k);
            
            sumList.Add(ExpandProduct([
                Num(coefficient), 
                Pow(f, Num(exponent - k)),
                ExpandPower(g, Num(k))
            ]));  
        }

        return Add(sumList).Simplify();
    }

    /// <summary>Return expanded form of two sum using the distributive 
    /// property of multiplication</summary>
    private static IExpression DistributeTerms(List<IExpression> first, List<IExpression> second)
    {
        List<IExpression> output = [];
        foreach (IExpression termFirst in first)
        {
            foreach (IExpression termSecond in second)
            {
                output.Add(Mul(termFirst, termSecond));
            }
        }
        return Add(output).Simplify();
    }
}