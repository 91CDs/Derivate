namespace nineT1CD;
using static nineT1CD.TokenType;
using static nineT1CD.Term;
public enum Sign
{
    Positive,
    Negative
}

public struct Term
{
    public Sign sign { get; set; }
    public int coefficient { get; set; }
    public string variable { get; set; } = "x";
    public int exponent { get; set; } = default;
    public Term(Sign sign, Literal cf)
    {
        this.sign = sign;
        coefficient = (int)cf.getValue();
    }
    public Term(Sign sign, Literal cf, Literal var)
    {
        this.sign = sign;
        coefficient = (int)cf.getValue();
        variable = var.value.ToString()!;
        exponent = 1;
    }
    public Term(Sign sign, Literal cf, Literal var, Literal exp)
    {
        this.sign = sign;
        coefficient = (int)cf.getValue();
        exponent = (int)exp.getValue();
        variable = var.value.ToString()!;
    }

    public string getSign()
    {
        return sign == Sign.Negative ? "-" : "+";
    }

    public Term reversedSign()
    {
        sign = sign == Sign.Positive ? Sign.Negative : Sign.Positive;
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
            output = new Term(sign, constant);
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
                output = new Term(sign, cfNode, var);
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
                    output = new Term(sign, cfNode, varNode, powNode);
                    return (isNumber(cfNode.type) && true)
                    || varNode.type == VAR 
                    || isNumber(powNode.type); 
                }
            }
        }
        
        output = default;
        return false;
    }

    public override string? ToString()
    {
        string exponentStr = exponent == 0 || exponent == 1
            ? String.Empty
            : $"^{exponent.ToString()}";
        string variableStr = exponent == 0
            ? String.Empty
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
        return String.Join("", 
            terms.Select((t, order) => 
            {
                var sign = order == 0 && t.sign == Sign.Positive 
                    ? String.Empty 
                    : $" {t.getSign()} ";
                return $"{sign}{t.ToString()}";
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