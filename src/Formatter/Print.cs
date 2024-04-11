using PrettyPrintTree;
namespace Derivate;
public static class PrintExtensions
{    
    public static void printTokens(this List<Token> tokens)
    {
        Console.WriteLine("\n:::::::::::::");
        Console.WriteLine("::TOKENIZER::");
        Console.WriteLine(":::::::::::::");
        Console.WriteLine(tokens.Format());
    }
    public static void printAST(this Node AST)
    {
        Console.WriteLine("\n:::::::");
        Console.WriteLine("::AST::");
        Console.WriteLine(":::::::");
        Console.WriteLine(AST.Format());
    }
    public static void printFunction(this Expression Function)
    {
        Console.WriteLine("\n::::::::::::");
        Console.WriteLine("::FUNCTION::");
        Console.WriteLine("::::::::::::");
        Console.WriteLine(Function.Format());
        Console.WriteLine(Function.ConvertToString());
    }

    public static void printFunctionTreePretty(this Expression func)
    {
        var tree = new PrettyPrintTree<Expression>(
            getChildren: (node) => node switch
            {
                Number 
                or Fraction 
                or Symbols  => [],
                Sum n        => n.value,
                Product n    => n.value,
                Power n      => [n.Base, n.Exponent],
                Function n   => [n.value],

                _ => throw new ArgumentOutOfRangeException(nameof(func)),
            },
            getVal: (node) => node switch
            {
                Number n     => n.value.ToString(),
                Fraction n   => $"({n.numerator} / {n.denominator})",
                Symbols n    => n.identifier.ToString(),
                Sum n        => "+",
                Product n    => "*",
                Power n      => "pow",
                Function n   => n.name,

                _ => throw new ArgumentOutOfRangeException(nameof(func)),
            },
            color: Color.NONE
        );
        tree.Display(func);
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
}