/*
TODO: Polynomial Simplification
* Adding Polynomials
    [x] + (num num ...)             = num + ... + num 
    [x] + (PolyN 0)                 = num
    [x] + (BiN)                     Like 
    [x] + (BiN)                     Unlike 
    [] + (PolyN)                    Like
    [] + (PolyN)                    Unlike
* Multiplying Polynomials
    [x] * (num num ...)             = num + ... + num
    [x] * (PolyN 0)                 = 0
    [x] * (PolyN 1)                 = PolyN
    [x] * (PolyN)                   Multiplying Terms
    [x] * (Exp Exp)                 Product Rule of Exponents
    [] * (+(PolyN) +(PolyN))        Foil Method
    [] * (+(PolyN) +(MonoN)^-1)     Separate Terms then do basic division
    [] * (+(PolyN) +(PolyN)^-1)     Long Division Method
* Exponential Polynomials
    [] num^num                      = num^num
    [] PolyN^0                      = 1
    [] PolyN^1                      = PolyN
    [] (a^m)^n                      Power of a Power Rule
    [] ab^n                         Power of a Product Rule
* Trigonometry
    [] sin(x) / cos(x)              = tan(x)
    [] 1 / sin(x), cos(x), tan(x)   = csc(x), sec(x), cot(x)
    [] sin(0), cos(0), tan(0)       = 0, 1, 0
    [] sin(π), cos(π), tan(π)       = 0, -1, 0
    [] sin(π/2), cos(π/2), tan(π/2) = 0, 1, UNDEFINED
    [] s(3π/2), c(3π/2), t(3π/2)    = 0, -1, UNDEFINED

[] Change layout to be based on operations instead of node content

** SIMPLIFIER: **


* Basic Program:
    While loop
        If output == previous output --> SIMPLIFIED
        Else (not the same)          --> NOT SIMPLIFIED (loop again)
* Problems 
    1. Associative Property
        [x] Tree Structure for + & * -> +[array] and *[array]
        :- Unary -  : -n  => -1 * n
        :- Binary - : m-n => m + (-1 * n)
        :- Binary / : m/n => m * n^-1
    2. Commutative Property (Polynomial Like Terms)
        [x] detect like terms and combine them
        :- +[array] sorting
        :- variable sorting based on order (ex: 3bca + 4acb => 3abc + 4abc)
            -> hash function for comparing terms (more performant)
    3. Distributive Property (multiple paths lead to same simplification)
        :- (FOR TRIG IDENTITIES) -> give user choice to "expand" or "factor"
        [] always choose simplification that decreases expr size (for trig identities)
        :- lmao too complicated
    4. Factoring Polynomials (univariate)
        :- for CMF -> use gcf algorithm
    5. Polynomial Long Division
        :- use the algorithm
    6. Simplifying Fractions
        :- too ez no sweat
*/
using System.Diagnostics;

namespace Derivate;

public static class Evaluator
{
    private static IExpression SimplifyFraction(Fraction f)
    {
        if (f.numerator % f.denominator == 0)
            return Func.Num(f.numerator / f.denominator);

        int gcd = MathInt.GCD(f.numerator, f.denominator);
        return f.denominator switch
        {
            > 0 => Func.Frac(f.numerator / gcd, f.denominator / gcd),
            // No negative denominator to avoid confusion
            < 0 => Func.Frac(-f.numerator / gcd, -f.denominator / gcd),
            0 => throw new UnreachableException($"{nameof(f.denominator)} cannot be 0"),
        };
    }
    private static IExpression SimplifySum(List<IExpression> function)
    {
        function = function.Select(Simplify).ToList();

        // Undefined
        if (function.Contains(Func.Undefined))
            return Func.Undefined;
        // Unary Sum
        if (function.Count == 1)
            return function.First();

        if (function.Count == 2)
            return (function[0], function[1]) switch
            {
                (Number l, Number r)     => l + r,
                (Fraction l, Fraction r) => SimplifyFraction(l + r),
                (Number l, Fraction r)   => SimplifyFraction(l + r),
                (Fraction l, Number r)   => SimplifyFraction(l + r), 

                // Identity Property
                (Number(0), var r) => r,
                (var l, Number(0)) => l,

                // Sum merging
                (Sum l, Sum r) => MergeSum(l.value, r.value),
                (Sum l, var r) => MergeSum(l.value, [r]),
                (var l, Sum r) => MergeSum([l], r.value),

                // c_1(x_1) + c_2(x_1) = (c_1 + c_2)(x_1) (Like Term merging)
                (var fx, var gx)
                when Func.Term(fx).Equals(Func.Term(gx))
                    => SimplifyProduct([
                        SimplifySum([Func.Const(fx), Func.Const(gx)]), 
                        Func.Term(fx)]),
                
                // Non Sum ordering
                (var l, var r) when !l.CompareTo(r)
                    => Func.Add(r, l),

                _ => Func.Add(function)
            };
        
        IExpression rest = SimplifySum(function.Skip(1).ToList());
        IEnumerable<IExpression> restList = rest is Sum expr ? expr.value : [rest];
        return function.First() switch
        {
            Sum p => MergeSum(restList, p.value),
            var p => MergeSum(restList, [p]),
        };
    }

    private static Sum MergeSum(IEnumerable<IExpression> a, IEnumerable<IExpression> b)
    {
        if (!a.Any())
            return Func.Add(b);

        if (!b.Any())
            return Func.Add(a);
        
        IExpression a1 = a.First(), b1 = b.First();
        return SimplifySum([a1, b1]) switch
        {
            Number(0) 
                => MergeSum(a.Skip(1), b.Skip(1)),
            Sum r when r == Func.Add(a1, b1)  
                => Func.Add(MergeSum(a.Skip(1), b).value.Prepend(a1)),
            Sum r when r == Func.Add(b1, a1) 
                => Func.Add(MergeSum(a, b.Skip(1)).value.Prepend(b1)),
            var r
                => Func.Add(MergeSum(a.Skip(1), b.Skip(1)).value.Prepend(r)),
        };
    }

    private static IExpression SimplifyProduct(List<IExpression> function)
    {
        function = function.Select(Simplify).ToList();

        // Undefined
        if (function.Contains(Func.Undefined))
            return Func.Undefined;
        // Zero Property
        if (function.Contains(Func.Num(0)))
            return Func.Num(0);
        // Unary Product
        if (function.Count == 1)
            return function.First();

        if (function.Count == 2)
            return (function[0], function[1]) switch
            {
                (Number l, Number r)     => l * r,
                (Fraction l, Fraction r) => SimplifyFraction(l * r),
                (Number l, Fraction r)   => SimplifyFraction(l * r),
                (Fraction l, Number r)   => SimplifyFraction(l * r), 

                // Identity Property
                (Number(1), var r) => r,
                (var l, Number(1)) => l,
                
                // Product merging
                (Product l, Product r) => MergeProduct(l.value, r.value),
                (Product l, var r)     => MergeProduct(l.value, [r]),
                (var l, Product r)     => MergeProduct([l], r.value),

                // Product Rule: u^m * u^n = u^m+n (Like term merging)
                (var fx, var gx)
                when Func.Base(fx).Equals(Func.Base(gx))
                    => SimplifyPow(
                        Func.Base(fx), 
                        SimplifySum([Func.Exponent(fx), Func.Exponent(gx)])),

                // Non product ordering
                (var l, var r) when !l.CompareTo(r)
                    => Func.Mul(r, l),
                
                _ => Func.Mul(function)
            };

        IExpression rest = SimplifyProduct(function.Skip(1).ToList());
        IEnumerable<IExpression> restList = rest is Product expr ? expr.value : [rest];
        return function.First() switch
        {
            Product p => MergeProduct(restList, p.value),
            var p => MergeProduct(restList, [p]),
        };
    }

    private static Product MergeProduct(IEnumerable<IExpression> a, IEnumerable<IExpression> b)
    {
        if (!a.Any())
            return Func.Mul(b);

        if (!b.Any())
            return Func.Mul(a);
        
        IExpression a1 = a.First(), b1 = b.First();
        return SimplifyProduct([a1, b1]) switch
        {
            Number(1) 
                => MergeProduct(a.Skip(1), b.Skip(1)),
            Product r when r == Func.Mul(a1, b1)  
                => Func.Mul(MergeProduct(a.Skip(1), b).value.Prepend(a1)),
            Product r when r == Func.Mul(b1, a1) 
                => Func.Mul(MergeProduct(a, b.Skip(1)).value.Prepend(b1)),
            var r
                => Func.Mul(MergeProduct(a.Skip(1), b.Skip(1)).value.Prepend(r)),
        };
    }
    private static IExpression SimplifyPow(IExpression left, IExpression right)
    {
        left = Simplify(left);
        right = Simplify(right);
        return (left, right) switch // (simplify(left), simplify(right)) breaks for some reason
        {
            // Undefined
            (_, Undefined) or (Undefined, _)
                => Func.Undefined,
            // 0^+n = 0
            (Number(0), Number r) when r.value > 0
                => Func.Num(0),
            // 0^(p/q) = 0
            (Number(0), Fraction) 
                => Func.Num(0),
            (Number(0), _)  
                => Func.Undefined,
            // 1^n = 1
            (Number(1), _)
                => Func.Num(1),
            (_, Number r)
                => SimplifyIntPow(left, r),
            _ => Func.Pow(left, right)
        };
    }
    private static IExpression SimplifyIntPow(IExpression expr, Number exponent)
    {
        return (expr, exponent) switch
        {
            // Power of Power: (u^n)^m = u^m*n
            (Power l, _) 
                => SimplifyProduct([l.Exponent, exponent]) switch
                {
                    Number pn => SimplifyIntPow(l.Base, pn),
                    var p     => Func.Pow(l.Base, p),
                },
            // Power of Product: (uv)^a = u^a * v^a
            (Product l, _) 
                => SimplifyProduct(
                    l.value.Select(x => SimplifyIntPow(x, exponent)).ToList()),
            // Numerical base and exponent
            (Number l, Number r) 
                => Number.Pow(l, r),
            // Power of Fraction
            (Fraction l, Number r)
                => Fraction.Pow(l, r),
            // Zero Property
            (_, Number(0))  
                => Func.Num(1), 
            // Identity Property
            (var l, Number(1))  
                => Simplify(l),
            _ => Func.Pow(expr, exponent) 
        };
    }
    private static IExpression SimplifyFunction(Function fx)
    {
        IExpression gx = Simplify(fx.value); 
        return gx switch
        {
            Undefined => Func.Undefined,
            _ => fx,
        };
    }
    
    public static IExpression Simplify(this IExpression expr)
    {
        return expr switch
        {
            Number     n => n,
            Symbols    n => n,
            Fraction   n => SimplifyFraction(n),
            Product    n => SimplifyProduct(n.value),
            Sum        n => SimplifySum(n.value),
            Power      n => SimplifyPow(n.Base, n.Exponent),
            Function   n => SimplifyFunction(n),

            _ => throw new ArgumentOutOfRangeException(nameof(expr)),
        };
    }
}