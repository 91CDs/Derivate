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
    public static string Format(this Expression func) 
    {
        return func switch 
        {
            Number n     => n.value.ToString(),
            Fraction n   => $"({n.numerator} / {n.denominator})",
            Symbols n    => n.identifier.ToString(),
            Sum n        => FormatSum(n),
            Product n    => FormatProduct(n),
            Power n      => FormatPower(n),
            Sine n       => $"sin({n.value.Format()})",
            Cosine n     => $"cos({n.value.Format()})",
            Tangent n    => $"tan({n.value.Format()})",
            Secant n     => $"sec({n.value.Format()})",
            Cosecant n   => $"csc({n.value.Format()})",
            Cotangent n  => $"cot({n.value.Format()})",
            Log n        => $"log({n.value.Format()})",
            NaturalLog n => $"ln({n.value.Format()})",

            _ => throw new ArgumentOutOfRangeException(nameof(func)),
        };  
    }

    public static string FormatPower(Power n)
    {
        return n switch
        {
            (var fx, Number(-1))
                => $"1 / {fx.Format()}",
            (var fx, Number(< -1) gx)
                => $"1 / {Func.Pow(fx, -gx).Format()}",
            (Number or Symbols, Number or Symbols)
                => $"{n.Base.Format()}^{n.Exponent.Format()}",
            (Product or Sum, Number or Symbols)
                => $"({n.Base.Format()})^{n.Exponent.Format()}",
            (Number or Symbols, Product or Sum)
                => $"{n.Base.Format()}^({n.Exponent.Format()})",
            _ => $"({n.Base.Format()})^({n.Exponent.Format()})",
        };
    }

    public static string FormatSum(Sum n)
    {
        StringBuilder str = new StringBuilder(n.value.First().Format());
        foreach (Expression operand in n.value.Skip(1))
        {
            string appendStr = operand switch
            {
                Product([Number(< 0) num, var p])
                    => $" - {(-num).Format()}{p.Format()}",
                Product([Number(< 0) num, .. var p])
                    => $" - ({(-num).Format()}{Func.Mul(p).Format()})",
                Product p
                    => $" + ({p.Format()})",
                var p 
                    => $" + {p.Format()}",
            };
            str.Append(appendStr);
        }
        return str.ToString();
    }
    public static string FormatProduct(Product n)
    {
        List<string> dividends = [];
        List<string> divisors = [];

        // Separate product operands to dividends and divisors
        for (int i = 0; i < n.value.Count; i++)
        {
            string appendStr = (i, n.value[i]) switch
            {
                (_, Power(var fx, Number(-1))) when fx is Product or Sum
                    => $"({fx.Format()})",
                (_, Power(var fx, Number(-1))) 
                    => $"{fx.Format()}",
                (_, Power(var fx, Number(< -1) gx)) when fx is Product or Sum
                    => $"({Func.Pow(fx, -gx).Format()})",
                (_, Power(var fx, Number(< -1) gx)) 
                    => $"{Func.Pow(fx, -gx).Format()}",
                (1, Power(Symbols,_) fx)
                    => $"{fx.Format()}",
                (1, Symbols fx)
                    => $"{fx.Format()}",
                (0, var fx)
                    => $"{fx.Format()}",
                (_, var fx)
                    => $"({fx.Format()})",
            };

            if (n.value[i] is Power(_, Number(< 0)))
                divisors.Add(appendStr);
            else
                dividends.Add(appendStr);
        }

        // Combine dividends and divisors to one string
        StringBuilder str = new StringBuilder();

        if (dividends.Any())
            str.Append($"{string.Concat(dividends)}");
        else
            str.Append('1');

        if (divisors.Any())
            str.Append($" / {string.Concat(divisors)}");

        return str.ToString();
    }
}