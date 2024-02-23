/*
Polynomial
    Term[]
        Unary(Coefficient * Variable ^ exponent)   | 3x^2 or log(3x)

TODO: Polynomial Simplification
[] Adding PolyN
   [x] + (num num)                 
   [x] + (a 0 or 0 a)                    = a
   [x] + (MonoN MonoN)             (basic) Like 
   [x] + (MonoN MonoN)             (basic) Unlike 
   [] + (PolyN PolyN)             Unlike
   [] + (MonoN MonoN .. PolyN)    Like
   [] + (PolyN MonoN .. PolyN)    Like
[] Subtracting PolyN [SAME IMPL]
   [x] - (num num)                 
   [x] - (a 0 or 0 a)                    = a
   [x] - (MonoN MonoN)             (basic) Like 
   [x] - (MonoN MonoN)             (basic) Unlike 
   [] - (PolyN PolyN)             Unlike
   [] - (MonoN MonoN .. PolyN)    Like
   [] - (PolyN MonoN .. PolyN)    Like
[] Multiplying PolyN
   [x] * (num num)                 
   [x] * (a 0 or 0 a)                    = 0
   [x] * (a 1 or 1 a)                    = a
   [x] * (MonoN MonoN)             (basic)
       [x] * (Exp Exp)             Product Rule of Exponents
   [] * (PolyN MonoN .. PolyN)    Foil Method
[] Dividing PolyN
   [x] / (num num)                
   [x] / (a 0)                    undefined
   [x] / (0 a)                    = 0
   [x] / (a 1)                    = a
   [x] / (MonoN MonoN)             (basic)
       [x] / (Exp Exp)             Quotient Rule of Exponents
   [] / (PolyN MonoN)             Separate Terms then do basic division
   [] / (PolyN PolyN)             Long Division Method
[] Change layout to be based on operations instead of node content

[] Exponential PolyN
   [x] num ^ num                  
   [x] 0 ^ a                      = 0
   [x] a ^ 0                      = 1
   [x] a ^ 1                      = a
   [x] a ^ -b                     Negative Power Rule
|------
   [] (a^e)^e                     Power of a Power Rule
   [] ab^e                        Power of a product Rule
   [] (a/b)^e                     Power of a quotient Rule
*/

using static Derivate.TokenType;

namespace Derivate;
public class Evaluator : NodeVisitor<Node>
{
    public Node eval(Node expr)
    {
        return expr.accept(this);
    }
    bool checkNumber(Node node, double expected)
    {
        if (!isNumber(node)) return false;

        double num = ((Literal)node).getValue();
        return num == expected;
    }
    bool checkNumber(Node node, Func<double, bool> func)
    {
        if (!isNumber(node)) return false;

        double num = ((Literal)node).getValue();
        return func(num);
    }
    bool isNumber(Node node)
    {
        var type = node.getType();
        return type == INT || type == FLOAT || type == CONST;
    }
    bool isVariable(Node node)
    {
        var type = node.getType();
        return type == VAR;
    }
    bool hasNumber(Node node)
    {
        var type = node.GetType();
        if (type == typeof(Grouping))
        {
            node = ((Grouping)node).expr;
            type = node.GetType();
        }

        if (type == typeof(Binary))
        {
            var binary = ((Binary)node);
            return isNumber(binary.left) || isNumber(binary.right);
        }

        return false;
    }
    bool isIdentityProperty(TokenType op, Node l, Node r, out Node identity)
    {
        if ((op == ADD || op == SUB) && checkNumber(l, 0)
        || (op == MUL || op == EXP) && checkNumber(l, 1))
        {
            identity = r;
            return true;
        }

        if ((op == ADD || op == SUB) && checkNumber(r, 0)
        || (op == MUL || op == DIV || op == EXP) && checkNumber(r, 1))
        {
            identity = l;
            return true;
        }

        identity = new Literal(0);
        return false;
    }

    bool isZeroProperty(TokenType op, Node l, Node r) =>
        (op == MUL || op == DIV) && checkNumber(l, 0)
        || (op == MUL) && checkNumber(r, 0);
    bool isUndefinedProperty(TokenType op, Node l, Node r) =>
        op == DIV && checkNumber(r, 0);
    bool isOneProperty(TokenType op, Node l, Node r) =>
        op == EXP && checkNumber(r, 0);
    Node calculateBinary(TokenType op, double num1, double num2, Token exprOp, Node expr)
    {
        return op switch
        {
            ADD => eval(new Binary(new Literal(num1 + num2), exprOp, expr)),
            SUB => eval(new Binary(new Literal(num1 - num2), exprOp, expr)),
            MUL => eval(new Binary(new Literal(num1 * num2), exprOp, expr)),
            DIV => eval(new Binary(new Literal(num1 / num2), exprOp, expr)),
            /* TODO: remove EXP */
            EXP => eval(new Binary(new Literal(Math.Pow(num1, num2)), exprOp, expr)), 

            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    Node evaluateBinaryGrouping(Binary nodeWithNum, Literal num, TokenType op)
    {
        Node expr;
        Token nodeOp = nodeWithNum.operation;
        double lnum = num.getValue(), rnum;
        
        if (isNumber(nodeWithNum.left))
        {
            rnum = ((Literal)nodeWithNum.left).getValue();
            expr = nodeWithNum.right;
            return calculateBinary(op, lnum, rnum, nodeOp, expr);
        }
        else if (isNumber(nodeWithNum.right))
        {
            rnum = ((Literal)nodeWithNum.right).getValue(); 
            expr = nodeWithNum.left;
            return calculateBinary(op, lnum, rnum, nodeOp, expr);
        }
        else 
        {
            return eval(nodeWithNum);
        }
    }

    Node evalExponent(Node left, Node right)
    {
        if (checkNumber(right, n => n < 0)) 
        {
            var num = -((Literal)right).getValue();
            return new Binary(
                new Literal(1), 
                new Token(DIV), 
                new Binary(
                    left, 
                    new Token(EXP),
                    new Literal(num)
                )
            );
        }

        if (isNumber(left))
        {
            var baseNum = ((Literal)left).getValue();
            Binary powerNode = (Binary)right;

            if (powerNode.getType() == MUL && isNumber(powerNode.left))
            {
                var power = ((Literal)powerNode.left).getValue();
                return new Binary(new Literal(Math.Pow(baseNum, power)), new Token(EXP), powerNode.right);
            }
        }
        
        if (isNumber(right))
        {
            var power = ((Literal)right).getValue();
            Binary baseNode = (Binary)left;
            var type = baseNode.getType();

            if (type == MUL || type == DIV && isNumber(baseNode.left))
            {
                var baseNum = ((Literal)baseNode.left).getValue();
                var baseExp = baseNode.right;
                
                return new Binary(
                    new Literal(Math.Pow(baseNum, power)), 
                    new Token(type), 
                    new Binary(
                        baseExp,
                        new Token(EXP),
                        new Literal(power)
                    )
                );
            }
        }

        return new Binary(left, new Token(EXP), right);
    }   

    public Node visitBinary(Binary binary)
    {
        Node left = eval(binary.left);
        Node right = eval(binary.right);
        TokenType op = binary.operation.type;

        if (isIdentityProperty(op, left, right, out Node identity)) return eval(identity);
        if (isZeroProperty(op, left, right)) return new Literal(0) ;
        if (isOneProperty(op, left, right)) return new Literal(1);
        if (isUndefinedProperty(op, left, right)) return new Literal(new Token(UNDEFINED, "undefined"));

        if (isNumber(left) && isNumber(right))
        {
            var lnum = ((Literal)left).getValue();
            var rnum = ((Literal)right).getValue();

            return op switch
            {
                ADD => new Literal(lnum + rnum),
                SUB => new Literal(lnum - rnum),
                MUL => new Literal(lnum * rnum),
                DIV => new Literal(lnum / rnum),
                EXP => new Literal(Math.Pow(lnum, rnum)),

                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (hasNumber(left) && isNumber(right) && (left.getType() == op))
        {
            return evaluateBinaryGrouping((Binary)left, (Literal)right, op);
        }
        if (hasNumber(right) && isNumber(left) && (right.getType() == op))
        {
            return evaluateBinaryGrouping((Binary)right, (Literal)left, op);
        }

        return new Binary(left, binary.operation, right);
    }


    public Node visitUnary(Unary unary)
    {
        Node right = eval(unary.right);
        TokenType op = unary.operation.type;

        if (isNumber(right))
        {
            var num = ((Literal)right).getValue();

            return op switch
            {
                SUB => new Literal(-num),
                SIN => new Literal(Math.Sin(num)),
                COS => new Literal(Math.Cos(num)),
                TAN => new Literal(Math.Tan(num)),
                CSC => new Literal(1/Math.Sin(num)),
                SEC => new Literal(1/Math.Cos(num)),
                COT => new Literal(1/Math.Tan(num)),
                LOG => new Literal(Math.Log(num)),
                LN  => new Literal(Math.Log(num)),

                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        
        return new Unary(unary.operation, right);
    }

    public Node visitLiteral(Literal literal)
    {
        return literal;
    }

    public Node visitGrouping(Grouping grouping)
    {
        return eval(grouping.expr);
    }
}