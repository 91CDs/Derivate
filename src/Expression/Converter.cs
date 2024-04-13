using System.Diagnostics;
using static Derivate.TokenType;

namespace Derivate;
public static class ConverterExtensions
{
    public static IExpression ToFunction(this Node node)
    {
        return node switch
        {
            Binary(ADD) n
                => combineAdd(n.left, n.right),
            Binary(SUB) n
                => Func.Add(
                    n.left.ToFunction(),
                    Func.Mul(Func.Num(-1), n.right.ToFunction())),
            Binary(MUL) n
                => combineMul(n.left, n.right),
            Binary(Literal(INT or FLOAT) a, DIV, Literal(INT or FLOAT) b) 
                => parseDivLiteral(a, b),
            Binary(DIV) n
                => Func.Mul(
                    n.left.ToFunction(),
                    Func.Pow(n.right.ToFunction(), Func.Num(-1))),
            Binary(EXP) n
                => Func.Pow(n.left.ToFunction(), n.right.ToFunction()),
            Unary(SUB)  n
                => Func.Mul(Func.Num(-1), n.right.ToFunction()),
            Unary(SIN)  n => Func.Sin(n.right.ToFunction()),
            Unary(COS)  n => Func.Cos(n.right.ToFunction()),
            Unary(TAN)  n => Func.Tan(n.right.ToFunction()),
            Unary(CSC)  n => Func.Csc(n.right.ToFunction()),
            Unary(SEC)  n => Func.Sec(n.right.ToFunction()),
            Unary(COT)  n => Func.Cot(n.right.ToFunction()),
            Unary(LOG)  n => Func.Log(n.right.ToFunction()),
            Unary(LN)   n => Func.Ln(n.right.ToFunction()),
            Symbol      n => Func.Var(n.value),
            Literal(INT) n
                => Func.Num((int) n.value),
            Literal(FLOAT) n
                => parseFloat(n),
            Literal(CONST) n 
                => Func.Var(Token.MathConstants(n.value)),

            _ => throw new ArgumentOutOfRangeException(nameof(node)),
        };
    }

    private static Constant parseDivLiteral(Literal a, Literal b)
    {        
        return (a, b) switch
        {
            (Literal(INT), Literal(INT))
                => Func.Frac((int) a.value, (int) b.value),
            (Literal(FLOAT), Literal(INT))
                => parseFloat(a) / Func.Num((int) b.value),
            (Literal(INT), Literal(FLOAT))
                => Func.Num((int) a.value) / parseFloat(b),
            (Literal(FLOAT), Literal(FLOAT))
                => parseFloat(a) / parseFloat(b),

            _ => throw new ArgumentOutOfRangeException($"{nameof(a)}{nameof(b)}"),
        };
    }

    private static Fraction parseFloat(Literal n)
    {
        Debug.Assert(n.type is FLOAT);
        return Func.Frac(
            parseNumerator(n.value), 
            parseDenominator(n.value));
    }
    
    private static int parseNumerator(double a)
    {
        if (double.IsInteger(a))
            return (int) a;

        string[] dec = a.ToString().Split(".");
        return int.Parse(string.Concat(dec));
    }

    private static int parseDenominator(double a)
    {
        if (double.IsInteger(a))
            return 1;
        
        string[] dec = a.ToString().Split(".");
        return (int) Math.Pow(10, dec.Last().Length);
    }

    private static IExpression combineMul(Node left, Node right)
    {
        List<IExpression> funcList = new();
        if (left is not Binary(MUL) l)
        {
            funcList.Add(left.ToFunction());
        }
        else
        {
            funcList.Add(l.left.ToFunction());
            funcList.Add(l.right.ToFunction());
        }

        if (right is not Binary(MUL) r)
        {
            funcList.Add(right.ToFunction());
        }
        else
        {
            funcList.Add(r.left.ToFunction());
            funcList.Add(r.right.ToFunction());
        }

        return Func.Mul(funcList);
    }

    private static IExpression combineAdd(Node left, Node right)
    {
        List<IExpression> funcList = new();
        if (left is not Binary(ADD) l)
        {
            funcList.Add(left.ToFunction());
        }
        else
        {
            funcList.Add(l.left.ToFunction());
            funcList.Add(l.right.ToFunction());
        }

        if (right is not Binary(ADD) r)
        {
            funcList.Add(right.ToFunction());
        }
        else
        {
            funcList.Add(r.left.ToFunction());
            funcList.Add(r.right.ToFunction());
        }

        return Func.Add(funcList);
    }
}