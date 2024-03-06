/*
Polynomial
    Term[]
        Unary(Coefficient * Variable ^ exponent)   | 3x^2 or log(3x)

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
        [] Tree Structure for + & * -> +[array] and *[array]
        :- Unary -  : -n  => -1 * n
        :- Binary - : m-n => m + (-1 * n)
        :- Binary / : m/n => m * n^-1
    2. Commutative Property (Polynomial Like Terms)
        [] detect like terms and combine them
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
using static Derivate.TokenType;

namespace Derivate;
public class Evaluator : NodeVisitor<Node>
{
    Node evalExp(Node left, Node right)
    {
        return (left, right) switch
        {
            // Power of Power: (a^n)^m = a^m*n
            (Binary (var baseNode, EXP, var powerOne), var powerTwo) 
            => Node.f(
                baseNode, 
                Token.EXP, 
                eval(Node.f(powerOne, Token.MUL, powerTwo))
            ),
            // Power of Product: (ab)^n = a^n * b^n
            (Binary (var numOne, MUL, var numTwo), var power)
            => Node.f(
                eval(Node.f(numOne, Token.EXP, power)),
                Token.MUL,
                eval(Node.f(numTwo, Token.EXP, power))
            ),
            (Literal l, Literal r) => Node.f(Math.Pow(l.value, r.value)),
            _ => Node.f(left, Token.EXP, right)
        };
    }  

    private Node evalDiv(Node left, Node right)
    {
        return (left, right) switch
        {
            (Literal l, Literal r) => Node.f(l.value / r.value),
            _ => Node.f(left, Token.DIV, right)
        };
    }

    private Node evalMul(Node left, Node right)
    {
        return (left, right) switch
        {
            // Product Rule: a^m * a^n = a^m+n
            (Binary (var baseOne, EXP, _) expOne, Binary (var baseTwo, EXP, _) expTwo) 
                when baseOne == baseTwo 
            => Node.f(expOne.left, 
                Token.EXP, 
                eval(Node.f(expOne.right, Token.ADD, expTwo.right))
            ),

            (Literal l, Literal r) => Node.f(l.value * r.value),
            _ => Node.f(left, Token.MUL, right)
        };
    }

    private Node evalSub(Node left, Node right)
    {
        return (left, right) switch
        {
            (Literal l, Literal r) => Node.f(l.value - r.value),
            _ => Node.f(left, Token.SUB, right)
        };
    }

    private Node evalAdd(Node left, Node right)
    {
        return (left, right) switch
        {
            (Literal l, Literal r) => Node.f(l.value + r.value),
            _ => Node.f(left, Token.ADD, right)
        };
    }
    
    // Visit functions //
    public Node eval(Node expr)
    {
        return expr.accept(this);
    }
    public Node visitBinary(Binary binary)
    {
        return binary.type switch
        {
            ADD => evalAdd(binary.left, binary.right),
            SUB => evalSub(binary.left, binary.right),
            MUL => evalMul(binary.left, binary.right),
            DIV => evalDiv(binary.left, binary.right),
            EXP => evalExp(binary.left, binary.right),

            _ => binary,
        };
    }


    public Node visitUnary(Unary unary)
    {
        Node right = eval(unary.right);
        TokenType op = unary.type;

        if (right is Literal literal)
        {
            return op switch
            {
                SUB => Node.f(-literal.value),
                SIN => Node.f(Math.Sin(literal.value)),
                COS => Node.f(Math.Cos(literal.value)),
                TAN => Node.f(Math.Tan(literal.value)),
                CSC => Node.f(1/Math.Sin(literal.value)),
                SEC => Node.f(1/Math.Cos(literal.value)),
                COT => Node.f(1/Math.Tan(literal.value)),
                LOG => Node.f(Math.Log(literal.value)),
                LN  => Node.f(Math.Log(literal.value)),

                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        
        return new Unary(unary.token, right);
    }

    public Node visitLiteral(Literal literal)
    {
        return literal;
    }
    public Node visitVariable(Variable variable)
    {
        return variable;
    }
}