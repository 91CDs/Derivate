using System.Diagnostics;
namespace Derivate;

public static class Derivative
{   
    public static Expression Dx(this Expression f)
    {
        // Given that f and g are functions and k ∈ R:
        Expression d = f switch
        {
            // Undefined Transformation
            Undefined 
                => Func.Undefined,
            // d/dx(k) = 0
            Constant n
                => Func.Num(0),
            // d/dx(x) = 1   
            Symbols n
                => Func.Num(1),
            // d/dx(f + g) = f' + g'
            Sum n
                => Func.Add(n.value.Select(Dx).ToList()),
            // d/dx(f * g) = fg' + f'g
            Product n
                => ProductDx(n.value),
            // d/dx(f^g) = f^g(gf'f^-1 + g'ln(f)) [GENERAL RULE]
            Power n
                => PowerDx(n.Base, n.Exponent),
            // d/dx(sin(f)) = cos(f)f'
            Sine n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Cos(n.value)),
            // d/dx(cos(f)) = -sin(f)f'
            Cosine n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Sub(Func.Sin(n.value))),
            // d/dx(tan(f)) = (sec(f)^2)f'
            Tangent n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Pow(Func.Sec(n.value), Func.Num(2))),
            // d/dx(sec(f)) = sec(x)tan(x)f'
            Secant n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Sec(n.value),
                    Func.Tan(n.value)),
            // d/dx(csc(f)) = -csc(f)cot(f)f'
            Cosecant n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Sub(Func.Csc(n.value)), 
                    Func.Cot(n.value)),
            // d/dx(cot(f)) = -(csc(f)^2)f'
            Cotangent n
                => Func.Mul(
                    Dx(n.value), 
                    Func.Sub(Func.Pow(Func.Csc(n.value), Func.Num(2)))),
            // d/dx(log(f)) = f' / (ln(10)f)
            Log n
                => Func.Mul(
                    Dx(n.value),  
                    Func.Div(Func.Mul(n.value, Func.Ln(Func.Num(10))))),
            // d/dx(nat(f)) = f' / f
            NaturalLog n
                => Func.Mul(
                    Dx(n.value),  
                    Func.Div(n.value)),
            
            _ => throw new UnreachableException(nameof(f)),
        };

        return d.Simplify();
    }

    private static Expression ProductDx(List<Expression> f)
    {
        return f switch
        {
            // Constant Product Rule
            // d/dx(kf) = kf'
            [var l, var r] when l is Number or Fraction 
                => Func.Mul(l, Dx(r)),
            [var l, .. var r] when l is Number or Fraction 
                => Func.Mul(l, Dx(Func.Mul(r))),
            
            // Quotient Rule
            // d/dx(f / g) = (f'g - fg') / g^2
            [var l, Power(var rbase, Number(< 0) rexp)] 
                => QuotientDx(l, Func.Pow(rbase, -rexp)),
            [.. var l, Power(var rbase, Number(< 0) rexp)] 
                => QuotientDx(Func.Mul(l), Func.Pow(rbase, -rexp)),
            [Power(var lbase, Number(< 0) lexp), var r] 
                => QuotientDx(r, Func.Pow(lbase, -lexp)),
            [Power(var lbase, Number(< 0) lexp), .. var r] 
                => QuotientDx(Func.Mul(r), Func.Pow(lbase, -lexp)),
            
            // Product Rule
            // d/dx(f * g) = fg' + f'g
            [var l, var r]   
                => Func.Add(
                Func.Mul(l, Dx(r)),
                Func.Mul(r, Dx(l))
            ),
            [var l, .. var r]   
                => Func.Add(
                Func.Mul(l, Dx(Func.Mul(r))),
                Func.Mul(Func.Mul(r), Dx(l))
            ),
            
            _ => throw new UnreachableException($"{nameof(f)} cannot be empty"),
        };
    }

    private static Expression QuotientDx(Expression l, Expression r)
    {
        return Func.Mul(
            Func.Add(
                Func.Mul(r, Dx(l)),
                Func.Sub(Func.Mul(l, Dx(r)))
            ),
            Func.Pow(r, Func.Num(-2))
        ); 
    }

    private static Expression PowerDx(Expression f, Expression g)
    {
        return (f, g) switch
        {
            // d/dx(x^n) = nx^n-1
            (_, Number or Fraction)
                => Func.Mul(
                    Dx(f),
                    g,
                    Func.Pow(f, Func.Add(g, Func.Num(-1)))),
            // d/dx(e^g) = (e^g)g'
            (E, _)
                => Func.Mul(
                    Dx(g),
                    Func.Pow(f, g)),
            // d/dx(f^g) = f^g(gf'f^-1 + g'ln(f))
            _   => Func.Mul(
                    Func.Pow(f, g),
                    Func.Add(
                        Func.Mul(g, Dx(f), Func.Div(f)),
                        Func.Mul(Dx(g), Func.Ln(f))))
        };
    }
}