using System.Text;
using static Derivate.TokenType;
namespace Derivate;

public static class FormatterExtensions
{
    public static string Format(this List<Token> tokens)
    {
        string tokensStr = string.Join(" , ", tokens.Select(t => t.ToString()));
        return $"[ {tokensStr} ]";
    }

    public static string Format(this Node node)
    {
        return node switch
        {
            Binary n    => $"({n.left.Format()}){n.value}({n.right.Format()})",
            Unary n     => $"{n.value}({n.right.Format()})",
            Literal(CONST, _) n => Token.MathConstants(n.value),
            Literal n   => n.value.ToString(),
            Symbol n  => n.value.ToString()!,

            _ => throw new ArgumentOutOfRangeException(nameof(node)),
        };
    }

    public static string Format(this IExpression func) 
    {
        return func switch 
        {
            Number n     => n.value.ToString(),
            Fraction n   => $"{n.numerator}/{n.denominator}",
            Symbols n    => n.identifier.ToString(),
            Sum n        => FormatSum(n),
            Product n    => FormatProduct(n),
            Power n      => FormatPower(n),
            Function n   => $"{n.name}({n.value.Format()})",

            _ => throw new ArgumentOutOfRangeException(nameof(func)),
        };  
    }

    public static string FormatParenthesis(IExpression n)
    {
        return n switch
        {
            Number or Symbols or Function => $"{n.Format()}",
            _ => $"({n.Format()})"
        };
    }

    public static string FormatPower(Power n)
    {
        string baseStr = FormatParenthesis(n.Base);
        string expStr = FormatParenthesis(n.Exponent);
        return $"{baseStr}^{expStr}";
    }

    public static string FormatProductParenthesis(int index, List<IExpression> product)
    {
        IExpression n = product[index];

        if (index == product.Count - 1)
        {
            if (n is Power(_, Number(> 0)) power) 
                return $"{FormatParenthesis(power.Base)}^{power.Exponent.Format()}";
            if (n is Power(var Base, Number(< -1) exp)) 
                return $"{FormatParenthesis(Base)}^{(-exp).Format()}";
        }

        if (index > 0 && n is Symbols)
        {
            IExpression nPrev = product[index - 1];
            if (nPrev is Symbols && 
                (n.Equals(nPrev)                                 // xx -> x*x
                || (n is not Variable && nPrev is not Variable)) // pii -> pi*i
            )
                return $"*{n.Format()}";
        }

        return (index, n) switch
        {
            (0, Number(-1))
                => "-",
            (0, Number)
                => $"{n.Format()}",
            (_, Number)
                => $"*{n.Format()}",
            (_, Power(_, Number(-1)) p)
                => FormatParenthesis(p.Base),
            (_, Power(_, Number(< -1) m) p)
                => FormatParenthesis(Func.Pow(p.Base, -m)),
            _ 
                => FormatParenthesis(n),
        };
    }

    public static string FormatProduct(Product n)
    {
        List<IExpression> dividends = [];
        List<IExpression> divisors = [];

        // Separate product operands to dividends and divisors
        for (int i = 0; i < n.value.Count; i++)
        {
            if (n.value[i] is Power(_, Number(< 0)))
            {
                divisors.Add(n.value[i]);
            }
            else
            {
                dividends.Add(n.value[i]);
            }
        }

        // Combine dividends and divisors to one string
        StringBuilder output = new StringBuilder();

        if (!dividends.Any())
        {
            output.Append('1');
        }
        else
        {
            for (int i = 0; i < dividends.Count; i++)
            {
                output.Append(FormatProductParenthesis(i, dividends));
            }
        }

        if (divisors.Any())
        {
            output.Append(" / ");
            for (int i = 0; i < divisors.Count; i++)
            {
                output.Append(FormatProductParenthesis(i, divisors));
            }
        }

        return output.ToString();
    }

    public static string FormatSumParenthesis(IExpression n)
    {
        return n switch
        {
            Product p 
            when p.value.Exists(op => op is Power(_, Number(< 0)))
                => $"({n.Format()})",
            Product or Power
                => $"{n.Format()}",
            _   => FormatParenthesis(n)
        };
    }

    public static string FormatSum(Sum n)
    {
        StringBuilder output = new StringBuilder();

        for (int i = 0; i < n.value.Count; i++)
        {
            string appendStr = (i, n.value[i]) switch
            {
                (0, Number(< 0) m)
                    => $"- {(-m).Format()}",
                (0, Product([Number(< 0) m, .. var p]))
                    => $"- {FormatSumParenthesis(Func.Mul([-m, ..p]))}",
                (0, var p)
                    => $"{FormatSumParenthesis(p)}",
                
                // Include space in each sign after first operand
                (_, Number(< 0) m)
                    => $" - {(-m).Format()}",
                (_, Product([Number(< 0) m, .. var p]))
                    => $" - {FormatSumParenthesis(Func.Mul([-m, ..p]))}",
                (_, var p)
                    => $" + {FormatSumParenthesis(p)}"

            };
            output.Append(appendStr);
        }

        return output.ToString();
    }
}