namespace nineT1CD;
using static nineT1CD.TokenType;
public record Func(Node f, Node df);

public class Derivative : NodeVisitor<Node>
{   
    bool checkType(Node n, params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (n.getType() == type)
                return true;
        }

        return false;
    }

    private Node SumDx(Node l, Node r)
    {
        return new Binary(dx(l), new Token(ADD), dx(r));
    }

    private Node DifferenceDx(Node l, Node r)
    {
        return new Binary(dx(l), new Token(SUB), dx(r));
    }

    public Node ProductDx(Node l, Node r)
    {
        if (checkType(l, INT, FLOAT, CONST))
        {
            return new Binary(l, new Token(MUL), dx(r));
        }

        return new Binary(
            new Binary(l, new Token(MUL), dx(r)),
            new Token(ADD),
            new Binary(r, new Token(MUL), dx(l))
        );
    }

    public Node QuotientDx(Node l, Node r)
    {
        return new Binary(
            new Binary(
                new Binary(r, new Token(MUL), dx(l)),
                new Token(SUB),
                new Binary(l, new Token(MUL), dx(r))
            ),
            new Token(DIV),
            new Binary(l, new Token(EXP), new Literal(2))
        ); 
    }

    public Node PowerDx(Node l, Node r)
    {
        if (checkType(r, INT))
        {
            var n = (int)((Literal)r).value;

            return new Binary(dx(l), new Token(MUL),
                new Binary(
                    new Literal(n),
                    new Token(MUL),
                    new Binary(
                        l,
                        new Token(EXP),
                        new Literal(n - 1)
                    )
                )
            );
        }

        if (checkType(l, CONST))
        {
            var n = (double)((Literal)l).value;
            
            if (n == Math.E)
                return new Binary( 
                    dx(l),
                    new Token(MUL),
                    new Binary(l, new Token(EXP), r)
                );
        }

        var log = new Binary(l, 
            new Token(MUL),
            new Unary(new Token(LN), r)
        );

        return new Binary(
            new Binary(
                new Literal(Math.E),
                new Token(EXP),
                log
            ),
            new Token(MUL),
            dx(log)
        );
    }

    public Node dx(Node expr)
    {
        return expr.accept<Node>(this);
    }

    public Node visitBinary(Binary binary)
    {
        var l = binary.left;
        var r = binary.right;

        return binary.operation.type switch
        {
            ADD => SumDx(l,r),
            SUB => DifferenceDx(l,r),
            MUL => ProductDx(l,r),
            DIV => QuotientDx(l,r),
            EXP => PowerDx(l,r),

            _ => throw new ArgumentException(),
        };
    }


    public Node visitUnary(Unary unary)
    {
        var r = unary.right;

        return unary.operation.type switch 
        {
            SIN => new Binary(dx(r), new Token(MUL), 
                    new Unary(new Token(COS), r)
                ),
            COS => new Binary(dx(r), 
                    new Token(MUL), 
                    new Unary(new Token(SUB), 
                        new Unary(new Token(SIN), r)
                    )
                ),
            TAN => new Binary(dx(r), 
                    new Token(MUL), 
                    new Binary(new Unary(new Token(SEC), r), 
                        new Token(EXP),
                        new Literal(2)
                    )
                ),
            CSC => new Binary(dx(r), 
                    new Token(MUL),
                    new Unary(new Token(SUB), 
                        new Binary(new Unary(new Token(CSC), r),
                            new Token(MUL),
                            new Unary(new Token(COT), r)
                        )
                    )
                ),
            SEC => new Binary(dx(r), 
                    new Token(MUL),
                    new Binary(new Unary(new Token(SEC), r),
                        new Token(MUL),
                        new Unary(new Token(TAN), r)
                    )
                ),
            COT => new Binary(dx(r), 
                    new Token(MUL), 
                    new Binary(new Unary(new Token(CSC), r), 
                        new Token(EXP),
                        new Literal(2)
                    )
                ),
            LOG => new Binary(dx(r), 
                    new Token(MUL), 
                    new Binary(new Literal(1), 
                        new Token(DIV),
                        new Binary(r,
                            new Token(MUL),
                            new Unary(new Token(LN), new Literal(10))
                        )
                    )
                ),
            LN =>  new Binary(dx(r), 
                    new Token(MUL), 
                    new Binary(new Literal(1), 
                        new Token(DIV),
                        r
                    )
                ),
            SUB =>  new Unary(new Token(SUB), dx(r)),

            _ => throw new ArgumentException(),
        };
    }

    public Node visitLiteral(Literal literal)
    {
        return literal.type switch
        {
            INT or FLOAT or CONST => new Literal(0),
            VAR => new Literal(1),

            _ => throw new ArgumentException(),
        };
    }

    public Node visitGrouping(Grouping grouping)
    {   
        return dx(grouping.expr);
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