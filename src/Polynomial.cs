using static Derivate.TokenType;
using static Derivate.Term;
namespace Derivate;
public enum Sign
{
    Positive,
    Negative
}

public struct Term
{
    public int coefficient { get; set; }
    public string variable { get; set; } = "x";
    public int exponent { get; set; } = default;
    public Term(Literal cf, Sign? sign = null)
    {
        var num = (int)cf.getValue();
        coefficient = sign == Sign.Negative ? -num : num;
    }
    public Term(Literal cf, Literal var, Sign? sign = null)
    {
        var num = (int)cf.getValue();
        coefficient = sign == Sign.Negative ? -num : num;
        variable = var.value.ToString()!;
        exponent = 1;
    }
    public Term(Literal cf, Literal var, Literal exp, Sign? sign = null)
    {
        var num = (int)cf.getValue();
        coefficient = sign == Sign.Negative ? -num : num;
        exponent = (int)exp.getValue();
        variable = var.value.ToString()!;
    }

    public Term(int cf, string var, int exp)
    {
        coefficient = cf;
        variable = var;
        exponent = exp;
    }

    public int calculateSign(Sign sign, int number)
    {
        return sign == Sign.Negative ? -number : number;
    }

    public Sign getSign()
    {
        return coefficient < 0 ? Sign.Negative : Sign.Positive;
    }

    public Term reversedSign()
    {
        coefficient = -coefficient;
        return this;
    }
    public static bool TryParseTerm(Node node, out Term output)
    {
        Sign sign = Sign.Positive;
        Binary? term, exp;
        Literal? constant, var, cfNode, varNode, powNode;
        bool isNumber(TokenType type) => type == INT || type == FLOAT || type == CONST; 

        Unary? neg = node as Unary;
        if (neg != null)
        {
            node = neg.right;
            sign = Sign.Negative;
        }
        /* EX: 3 */
        constant = node as Literal;
        if (constant != null) 
        {
            output = new Term(constant, sign);
            return isNumber(constant.type);
        }

        term = node as Binary;
        if (term != null) 
        {
            cfNode = term.left as Literal;
            
            /* EX: 3x */
            var = term.right as Literal;
            if (var != null && cfNode != null)
            {
                output = new Term(cfNode, var, sign);
                return (isNumber(cfNode.type) && true)|| var.type == VAR;
            }

            /* EX: 3x^2 */
            exp = term.right as Binary;
            if (exp != null && cfNode != null)
            {
                varNode = exp.left as Literal;
                powNode = exp.right as Literal;

                if (varNode != null && powNode != null)
                {
                    output = new Term(cfNode, varNode, powNode, sign);
                    return (isNumber(cfNode.type) && true)
                    || varNode.type == VAR 
                    || isNumber(powNode.type); 
                }
            }
        }
        
        output = default;
        return false;
    }

    public static bool isLikeTerm(Term a, Term b)
    {
        return a.variable == b.variable && a.exponent == b.exponent;
    }

    public static Term operator +(Term a, Term b)
    {
        if (!isLikeTerm(a,b))
            throw new ArgumentException("The two terms are not like terms.");

        return new Term(a.coefficient + b.coefficient, a.variable, a.exponent);
    }
    public static Term operator -(Term a, Term b)
    {
        if (!isLikeTerm(a,b))
            throw new ArgumentException("The two terms are not like terms.");

        return new Term(a.coefficient - b.coefficient, a.variable, a.exponent);
    }
    public static Term operator *(Term a, Term b)
    {
        return new Term(a.coefficient * b.coefficient, a.variable, a.exponent + b.exponent);
    }
    public static Term operator /(Term a, Term b)
    {
        return new Term(a.coefficient / b.coefficient, a.variable, a.exponent - b.exponent);
    }

    public override string? ToString()
    {
        string exponentStr = exponent == 0 || exponent == 1
            ? string.Empty
            : $"^{exponent}";
        string variableStr = exponent == 0
            ? string.Empty
            : variable;
        return $"{coefficient}{variableStr}{exponentStr}";
    }
}

public struct Polynomial
{
    public Term[] terms { get; set; }
    public uint degree { get; set; } = 0;
    public Polynomial(Term[] terms, uint degree)
    {
        this.terms = terms;
        this.degree = degree;
    }

    public static bool TryParsePolynomial(Node node, out Polynomial output)
    {
        var (isPolynomial, polynomialOutput) = new PolynomialParser().TryParsePolynomial(node);
        uint polynomialDegree = (uint)polynomialOutput
            .FindAll(t => t.exponent >= 0)
            .MaxBy(t => t.exponent).exponent;
        
        output = new Polynomial(polynomialOutput.ToArray(), polynomialDegree);
        return isPolynomial;
    }

    public override string? ToString()
    {
        return string.Join("", 
            terms.Select((t, order) => 
            {
                var sign = order == 0 && t.getSign() == Sign.Positive
                    ? string.Empty 
                    : $" {(t.getSign() == Sign.Positive ? "+" : "-")} ";
                return $"{sign}{t}";
            })
        );
    }
}

public class PolynomialParser : NodeVisitor<(bool, List<Term>)>
{
    public (bool, List<Term>) TryParsePolynomial(Node node)
    {
        return node.accept(this);
    }

    public (bool, List<Term>) visitBinary(Binary binary)
    {
        List<Term> newTerms = new List<Term>();
        bool isPolynomial = false;

        if (TryParseTerm(binary, out Term term)) 
        {
            isPolynomial = true;
            newTerms.Add(term);
        }

        if (Node.checkType(binary, ADD, SUB))
        {
            var (leftIsPolynomial, left) = TryParsePolynomial(binary.left);
            var (rightIsPolynomial, right) = TryParsePolynomial(binary.right);
            if (leftIsPolynomial && rightIsPolynomial) 
            {
                isPolynomial = true;
                newTerms.AddRange(left);

                if (Node.checkType(binary, SUB)) 
                {
                    right = right.Select(t => t.reversedSign()).ToList();
                }
                newTerms.AddRange(right);
            }
        }

        return (isPolynomial, newTerms);
    }

    /* TODO: unary( polynomial ) case => reverse all signs inside */
    public (bool, List<Term>) visitUnary(Unary unary)
    {
        List<Term> newTerms = new List<Term>();
        bool isPolynomial = false;

        if (TryParseTerm(unary, out Term term))
        {
            isPolynomial = true;
            newTerms.Add(term);
        }

        var (exprIsPolynomial, expr) = TryParsePolynomial(unary.right);
        if (exprIsPolynomial)
        {
            isPolynomial = true;

            expr = expr.Select(t => t.reversedSign()).ToList();
            newTerms.AddRange(expr);
        }

        return (isPolynomial, newTerms);
    }

    public (bool, List<Term>) visitGrouping(Grouping grouping)
    {
        return TryParsePolynomial(grouping.expr);
    }

    public (bool, List<Term>) visitLiteral(Literal literal)
    {
        List<Term> newTerms = new List<Term>();
        bool isPolynomial = false;

        if (TryParseTerm(literal, out Term term))
        {
            isPolynomial = true;
            newTerms.Add(term);
        }

        return (isPolynomial, newTerms);
    }
}

/* 
public class PolynomialGetter : NodeVisitor<List<Term>?>
{
    public List<Term>? getPolynomial(Node polynomial)
    {
        return polynomial.accept(this);
    }
    public List<Term>? visitBinary(Binary binary)
    {
        List<Term> newTerms = new List<Term>();

        if (isTerm(binary)) 
        {
            Term term = new Term(binary);
            newTerms.Add(term);
        }

        while (Node.checkType(binary, ADD, SUB))
        {
            if (isTerm(binary.left)) 
            {
                newTerms.Add(new Term(binary.left));
            }
            
            if (isPolynomial(binary.right))
            {
                var terms = getPolynomial(binary.right);
                newTerms.Add()
            }
        }

        return newTerms;
        
    }
    public List<Term>? visitGrouping(Grouping grouping)
    {
        return getPolynomial(grouping.expr);
    }
    public List<Term>? visitUnary(Unary unary)
    {
        List<Term> newTerms = new List<Term>();
        if (isTerm(unary))
            newTerms.Add(new Term(SUB, unary));

        return newTerms;
    }

    public List<Term>? visitLiteral(Literal literal)
    {
        return null;
    }
} */