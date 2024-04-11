using System.Diagnostics;
namespace Derivate;

public static class Derivative
{   
    private static Expression ProductDx(List<Expression> f)
    {
        return f switch
        {
            // Constant Product Rule
            [var l, var r] when l is Number or Fraction 
                => Func.Mul(l, dx(r)),
            [var l, .. var r] when l is Number or Fraction 
                => Func.Mul(l, dx(Func.Mul(r))),
            
            // Quotient Rule
            [var l, Power(var rbase, Number(< 0) rexp)] 
                => QuotientDx(l, Func.Pow(rbase, -rexp)),
            [.. var l, Power(var rbase, Number(< 0) rexp)] 
                => QuotientDx(Func.Mul(l), Func.Pow(rbase, -rexp)),
            [Power(var lbase, Number(< 0) lexp), var r] 
                => QuotientDx(r, Func.Pow(lbase, -lexp)),
            [Power(var lbase, Number(< 0) lexp), .. var r] 
                => QuotientDx(Func.Mul(r), Func.Pow(lbase, -lexp)),
            
            // Product Rule
            [var l, var r]   
                => Func.Add(
                Func.Mul(l, dx(r)),
                Func.Mul(r, dx(l))
            ),
            [var l, .. var r]   
                => Func.Add(
                Func.Mul(l, dx(Func.Mul(r))),
                Func.Mul(Func.Mul(r), dx(l))
            ),
            
            _ => throw new UnreachableException($"{nameof(f)} cannot be empty"),
        };
    }

    private static Expression QuotientDx(Expression l, Expression r)
    {
        return Func.Mul(
            Func.Add(
                Func.Mul(r, dx(l)),
                Func.Sub(Func.Mul(l, dx(r)))
            ),
            Func.Pow(r, Func.Num(-2))
        ); 
    }

    private static Expression PowerDx(Expression l, Expression r)
    {
        var log = Func.Mul(l, Func.Ln(r));

        return (l, r) switch
        {
            (_, Number or Fraction)
                => Func.Mul(
                    dx(l),
                    r,
                    Func.Pow(l, Func.Add(r, Func.Num(-1)))),
            (E, _)
                => Func.Mul(
                    dx(r),
                    Func.Pow(l, r)),
            _   => Func.Mul(
                    Func.Pow(Func.E, log),
                    dx(log))
        };
    }

    public static Expression dx(this Expression f)
    {
        Expression d = f switch
        {
            Constant n     
                => Func.Num(0),
            Undefined 
                => Func.Undefined,
            Symbols n        
                => Func.Num(1),
            Sum n        
                => Func.Add(n.value.Select(dx).ToList()),
            Product n   
                => ProductDx(n.value),
            Power n      
                => PowerDx(n.Base, n.Exponent),
            Sine n       
                => Func.Mul(
                    dx(n.value), 
                    Func.Cos(n.value)),
            Cosine n     
                => Func.Mul(
                    dx(n.value), 
                    Func.Sub(Func.Sin(n.value))),
            Tangent n    
                => Func.Mul(
                    dx(n.value), 
                    Func.Pow(Func.Sec(n.value), Func.Num(2))),
            Secant n     
                => Func.Mul(
                    dx(n.value), 
                    Func.Sec(n.value),
                    Func.Tan(n.value)),
            Cosecant n   
                => Func.Mul(
                    dx(n.value), 
                    Func.Sub(Func.Csc(n.value)), 
                    Func.Cot(n.value)),
            Cotangent n  
                => Func.Mul(
                    dx(n.value), 
                    Func.Sub(Func.Pow(Func.Csc(n.value), Func.Num(2)))),
            Log n        
                => Func.Mul(
                    dx(n.value),  
                    Func.Div(Func.Mul(n.value, Func.Ln(Func.Num(10))))),
            NaturalLog n 
                => Func.Mul(
                    dx(n.value),  
                    Func.Div(n.value)),
            
            _ => throw new UnreachableException(nameof(f)),
        };
        return d.simplify();
    }
}

/* 
|-----
|                                                             
|  INT, FLOAT, CONST                                            
|  { d/dx(k) = 0 }                                            
|                                                             
|  VAR                                                        
|  { d/dx(x) = 1 }                                            
|                                                       
|  Neg            => (SUB Expr)                               
|  { d/dx(-f(x)) = -f'(x) }                                   
|                                                             
|  SIN            => (SIN Expr)                               
|  { d/dx(sin(f(x))) = cos(f(x)) * f'(x) }                    
|                                                             
|  COS            => (COS Expr)                               
|  { d/dx(cos(f(x))) = -sin(f(x)) * f'(x) }                   
|                                                             
|  TAN            => (TAN Expr)                               
|  { d/dx(tan(f(x))) = sec(f(x))^2 * f'(x) }                  
|                                                             
|  CSC            => (CSC Expr)                               
|  { d/dx(csc(f(x))) = -csc(f(x))cot(f(x)) * f'(x) }          
|                                                             
|  SEC            => (SEC Expr)                               
|  { d/dx(sec(f(x))) = sec(f(x))tan(f(x)) * f'(x) }           
|                                                             
|  COT            => (COT Expr)                               
|  { d/dx(cot(f(x))) = csc(f(x))^2 * f'(x) }                  
|                                                             
|  LN             => (LN  Expr)                               
|  { d/dx(ln(f(x))) = 1/f(x) * f(x) }                         
|                                                             
|  LOG            => (LOG Expr)                               
|  { d/dx(log(f(x))) = 1/f(x)ln(10) * f(x) }                  
|-----                                                        
|  Sum            => (Expr ADD  Expr)                         
|  { d/dx(f(x) + g(x)) = f'(x) + g'(x) }                      
|-----                                                        
|  Difference     => (Expr SUB  Expr)                         
|  { d/dx(f(x) - g(x)) = f'(x) - g'(x) }                      
|-----                                                        
|  Constant Mul   => (Num  MUL  Expr)                         
|  { d/dx(k * f(x)) = k * f'(x) }                             
|                                                             
|  Product        => (Expr MUL  Expr)                         
|  { d/dx(f(x) * g(x)) = f(x)g'(x) + g(x)f'(x) }              
|-----                                                        
|  Quotient       => (Expr DIV  Expr)                         
|  { d/dx(f(x) / g(x)) = g(x)f'(x) - f(x)g'(x) / g(x)^2}      
|-----                                                        
|  Power          => (var  EXP  Num )                         
|  { d/dx(x^n) = nx^(n-1)}                                    
|                                                             
|  General Power  => (Expr EXP  Num )                         
|  { d/dx((f(x))^n) = n(f(x))^(n-1) * f'(x) }                 
|                                                             
|  Natural Exp    => (e    EXP  Expr)                         
|  { d/dx(e^f(x)) = e^f(x) * f'(x)}                           
|                                                             
|  General Exp    => (Num  EXP  Expr)                         
|  { d/dx(n^f(x)) = (n^f(x))ln(n) * f'(x)}                    
|                                                             
|  Power log      => (Expr EXP  Expr)                         
|  { d/dx(f(x)^g(x)) = e^f(x)ln(g(x)) * d/dx(f(x)ln(g(x)))}   
|                                                             
|-----
 */