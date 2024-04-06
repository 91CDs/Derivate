using System.Text;
using static Derivate.TokenType;
namespace Derivate;
public static class PrintExtensions
{    
    public static void printTokens(this List<Token> tokens)
    {
        Console.WriteLine("\nTOKENIZER::");
        Console.WriteLine(":::::::::::");
        Console.WriteLine(tokens.Format());
    }
    public static void printAST(this Node AST)
    {
        Console.WriteLine("\nAST::");
        Console.WriteLine(":::::");
        Console.WriteLine(AST.Format());
    }
    public static void printFunction(this Expression Function)
    {
        Console.WriteLine("\nFUNCTION::");
        Console.WriteLine("::::::::::");
        Console.WriteLine(Function.Format());
        Console.WriteLine(Function.ConvertToString());
    }

    public static string ConvertToString(this Expression func)
    {
        return func switch
        {
            Number n     => n.value.ToString(),
            Fraction n   => $"({n.numerator}/{n.denominator})",
            Symbols n    => n.identifier.ToString(),
            Product n    => $"*[{string.Join(", ", n.value.Select(n => n.ConvertToString()))}]",
            Sum n        => $"+[{string.Join(", ", n.value.Select(n => n.ConvertToString()))}]",
            Power n      => $"({n.Base.ConvertToString()}^{n.Exponent.ConvertToString()})",
            Sine n       => $"sin({n.value.ConvertToString()})",
            Cosine n     => $"cos({n.value.ConvertToString()})",
            Tangent n    => $"tan({n.value.ConvertToString()})",
            Secant n     => $"sec({n.value.ConvertToString()})",
            Cosecant n   => $"csc({n.value.ConvertToString()})",
            Cotangent n  => $"cot({n.value.ConvertToString()})",
            Log n        => $"log({n.value.ConvertToString()})",
            NaturalLog n => $"ln({n.value.ConvertToString()})",

            _ => throw new ArgumentOutOfRangeException(nameof(func)),
        };
    }
    
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
            Sum n        => string.Join(" + ", n.value.Select(n => n.Format())),
            Product n    => FormatMultiply(n),
            Power n      => $"{n.Base.Format()}^{n.Exponent.Format()}",
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
    public static string FormatMultiply(Product n)
    {
        if (n.value.Count == 1)
            return n.value.First().Format();

        if (n.value.Count == 2)
            return (n.value[0], n.value[1]) switch
            {
                (Number fx, Power(Variable,_) gx)
                    => $"{fx.Format()}{gx.Format()}",
                (Number fx, Variable gx)
                    => $"{fx.Format()}{gx.Format()}",
                (Number fx, Function gx)
                    => $"{fx.Format()}{gx.Format()}",
                (var fx, var gx)
                    => $"{fx.Format()}({gx.Format()})",
            };

        StringBuilder str = new StringBuilder();
        for (int i = 0; i < n.value.Count; i++)
        {
            string appendStr = (i, n.value[i]) switch
            {
                (_, Power(Variable,_) fx) => $"{fx.Format()}",
                (_, Variable fx)          => $"{fx.Format()}",
                (0, var fx)          => $"{fx.Format()}",
                (_, var fx)          => $"({fx.Format()})",
            };
            str.Append(appendStr);
        }
        return str.ToString();
    }
}