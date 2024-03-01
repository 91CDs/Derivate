using System.Diagnostics;
using static Derivate.TokenType;
namespace Derivate;

public class Derivative : NodeVisitor<Node>
{   
    private Node SumDx(Node l, Node r)
    {
        return Node.f(dx(l), Token.ADD, dx(r));
    }

    private Node DifferenceDx(Node l, Node r)
    {
        return Node.f(dx(l), Token.SUB, dx(r));
    }

    private Node ProductDx(Node l, Node r)
    {
        if (l is Literal)
        {
            return Node.f(l, Token.MUL, dx(r));
        }

        return Node.f(
            Node.f(l, Token.MUL, dx(r)),
            Token.ADD,
            Node.f(r, Token.MUL, dx(l))
        );
    }

    private Node QuotientDx(Node l, Node r)
    {
        return Node.f(
            Node.f(
                Node.f(r, Token.MUL, dx(l)),
                Token.SUB,
                Node.f(l, Token.MUL, dx(r))
            ),
            Token.DIV,
            Node.f(r, Token.EXP, Node.f(2))
        ); 
    }

    private Node PowerDx(Node l, Node r)
    {
        if (r is Literal(INT or FLOAT, _) integer)
        {
            var n = (int)integer.value;

            return Node.f(dx(l), Token.MUL,
                Node.f(
                    Node.f(n),
                    Token.MUL,
                    Node.f(
                        l,
                        Token.EXP,
                        Node.f(n - 1)
                    )
                )
            );
        }

        if (l is Literal(CONST, var num) && num is Math.E)
        {
            return Node.f( 
                dx(r),
                Token.MUL,
                Node.f(l, Token.EXP, r)
            );
        }

        var log = Node.f(l, 
            Token.MUL,
            Node.f(Token.LN, r)
        );

        return Node.f(
            Node.f(
                Node.f(Math.E),
                Token.EXP,
                log
            ),
            Token.MUL,
            dx(log)
        );
    }

    public Node dx(Node expr)
    {
        return expr.accept(this);
    }

    public Node visitBinary(Binary binary)
    {
        var l = binary.left;
        var r = binary.right;

        return binary.type switch
        {
            ADD => SumDx(l,r),
            SUB => DifferenceDx(l,r),
            MUL => ProductDx(l,r),
            DIV => QuotientDx(l,r),
            EXP => PowerDx(l,r),

            _ => throw new UnreachableException(),
        };
    }


    public Node visitUnary(Unary unary)
    {
        var r = unary.right;

        return unary.type switch 
        {
            SIN => Node.f(dx(r), Token.MUL, Node.f(Token.COS, r)),
            COS => Node.f(dx(r), 
                    Token.MUL, 
                    Node.f(Token.SUB, Node.f(Token.SIN, r))
                ),
            TAN => Node.f(dx(r), 
                    Token.MUL, 
                    Node.f(Node.f(Token.SEC, r), 
                        Token.EXP,
                        Node.f(2)
                    )
                ),
            CSC => Node.f(dx(r), 
                    Token.MUL,
                    Node.f(Token.SUB, 
                        Node.f(Node.f(Token.CSC, r),
                            Token.MUL,
                            Node.f(Token.COT, r)
                        )
                    )
                ),
            SEC => Node.f(dx(r), 
                    Token.MUL,
                    Node.f(Node.f(Token.SEC, r),
                        Token.MUL,
                        Node.f(Token.TAN, r)
                    )
                ),
            COT => Node.f(dx(r), 
                    Token.MUL, 
                    Node.f(Token.SUB, 
                        Node.f(Node.f(Token.CSC, r), 
                            Token.EXP,
                            Node.f(2)
                        )
                    )
                ),
            LOG => Node.f(dx(r), 
                    Token.MUL, 
                    Node.f(Node.f(1), 
                        Token.DIV,
                        Node.f(r,
                            Token.MUL,
                            Node.f(Token.LN, Node.f(10))
                        )
                    )
                ),
            LN =>  Node.f(dx(r), 
                    Token.MUL, 
                    Node.f(Node.f(1), 
                        Token.DIV,
                        r
                    )
                ),
            SUB =>  Node.f(Token.SUB, dx(r)),

            _ => throw new UnreachableException(),
        };
    }

    public Node visitLiteral(Literal literal)
    {
        return Node.f(0);
    }

    public Node visitVariable(Variable variable)
    {
        return Node.f(1);
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