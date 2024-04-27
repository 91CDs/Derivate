namespace Derivate;

public static class GeneralPolynomial
{
    /// <summary>Checks if expression is a general monomial 
    /// expression (GME)</summary>
    public static bool isGeneralMonomial(this IExpression expr, HashSet<IExpression> variables)
    {
        if (variables.Contains(expr))
            return true;
        if (expr.FreeOf(variables))
            return true;

        return expr switch
        {
            Power a 
            when variables.Contains(a.Base) 
            && a.Exponent is Number(> 1)
                => true,
            Product a 
            when a.value.All(op => isGeneralMonomial(op, variables))
                => true,

            _ => false
        };
    }

    /// <summary>Checks if expression is a general polynomial 
    /// expression (GPE)</summary>
    public static bool isGeneralPolynomial(this IExpression expr, HashSet<IExpression> variables)
    {
        return expr switch
        {
            Sum a
            when a.value.All(op => isGeneralMonomial(op, variables))
                => true,

            _ => isGeneralMonomial(expr, variables)
        };
    }

    /// <summary>Returns a list of all variables found in the GPE</summary>
    public static HashSet<IExpression> GetVariables(this IExpression expr)
    {
        return expr switch
        {
            Constant 
                => [],
            Power a when a.Exponent is Number(> 1) 
                => [a.Base],
            Sum a
                => a.value.Select(GetVariables)
                .Aggregate((vars, next) => [.. vars, .. next]),
            Product a
                => a.value.FindAll(op => op is not Sum)
                .Select(GetVariables)
                .Aggregate((vars, next) => [.. vars, .. next])
                .Concat(a.value.FindAll(op => op is Sum))
                .ToHashSet(),

            _ => [expr]
        };
    }

    /// <summary>Returns the coefficient of a GME with respect to the
    /// set variable or Undefined if expr is not a GPE</summary>
    public static (IExpression cf, int deg) MonomialCoefficient(
        this IExpression term,
        IExpression variable)
    {
        if (term.Equals(variable))
            return (Func.Num(1), 1);
        if (term.FreeOf(variable))
            return (term, 0);

        if (term is Product product)
        {
            (Product cf, int deg) = (product, 0);
            foreach (IExpression operand in product.value)
            {
                var (coeff, degree) = operand.MonomialCoefficient(variable);
                if (coeff.Equals(Func.Undefined))
                    return (Func.Undefined, 0);

                if (degree != 0)
                {
                    deg = degree; 
                    cf = Func.Mul(cf.value.FindAll(op =>
                        !op.Equals(
                            Func.Pow(variable, Func.Num(degree)).Simplify())
                    ));
                }
            }

            if (cf.value.Count == 1)
                return (cf.value[0], deg);

            return (cf, deg);
        }
        
        return term switch
        {
            Power pow 
            when pow.Base.Equals(variable) 
            && pow.Exponent is Number(> 1) n
                => (Func.Num(1), n.value),
            
            _ => (Func.Undefined, 0)
        };
    }

    /// <summary>Returns the coefficient of a GPE with respect to the
    /// set variable and degree or Undefined if expr is not a GPE</summary>
    public static IExpression PolynomialCoefficient(
        this IExpression expr,
        IExpression variable,
        int degree)
    {
        if (expr.Equals(variable))
            return degree == 1 ? Func.Num(1) : Func.Num(0);
        
        if (expr is Sum sum)
        {
            List<IExpression> cfList = [];
            foreach (IExpression operand in sum.value)
            {
                (IExpression cf, int deg) = operand.MonomialCoefficient(variable);
                if (cf.Equals(Func.Undefined))
                    return Func.Undefined;
                
                if (degree == deg)
                    cfList.Add(cf);
            }

            if (cfList.Count == 1)
                return cfList[0];

            return Func.Add(cfList);
        }

        (IExpression cfExpr, int degExpr) = expr.MonomialCoefficient(variable);
        if (cfExpr.Equals(Func.Undefined))
            return Func.Undefined;

        return degree == degExpr ? cfExpr : Func.Num(0);
    }

    /// <summary>Returns the leading coefficient of a GPE with respect to the
    /// set variable or Undefined if expr is not a GPE</summary>
    public static IExpression LeadingCoefficient(
        this IExpression expr,
        IExpression variable)
    {
        int degree = expr.Degree([variable]);
        return expr.PolynomialCoefficient(variable, degree);
    }

    /// <summary>Returns the degree of a GME with respect to the set 
    /// variables or 0 when term is not a GPE</summary>
    public static int MonomialDegree(this IExpression term, HashSet<IExpression> variables)
    {
        if (!term.isGeneralMonomial(variables))
            return 0;
        if (variables.Contains(term.Base()))
            return term.Exponent() is Number n ? n.value : 0;
        
        return term switch
        {
            Product p => p.value
                .FindAll(op => variables.Contains(op.Base()))
                .Select(op => op.Exponent() is Number n ? n.value : 0)
                .Sum(),
            _ => 0
        };

    }

    /// <summary>Returns the degree of a polynomial with respect to the set 
    /// variables or 0 when expr is not a GPE</summary>
    public static int Degree(this IExpression expr, HashSet<IExpression> variables)
    {
        if (!expr.isGeneralPolynomial(variables))
            return 0;

        return expr switch
        {
            Sum s => s.value
                .Select(term => MonomialDegree(term, variables))
                .Max(),
            _ => MonomialDegree(expr, variables)
        };
    }

    /// <summary>Divides a univariate polynomial by another univariate 
    /// polynomial</summary>
    /// <param name="dividend">The univariate polynomial to be divided</param>
    /// <param name="divisor">The univariate polynomial divisor</param>
    /// <returns>A tuple (q, r) where q is quotient and r is remainder</returns>
    public static (IExpression, IExpression) DivRem(IExpression dividend, IExpression divisor)
    {
        throw new NotImplementedException();
    }
}