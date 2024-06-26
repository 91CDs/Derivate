using PrettyPrintTree;
namespace Derivate;
public static class PrintExtensions
{
    public static void DebugPrint(this List<Token> tokens)
    {
        Console.WriteLine("\n:::::::::::::");
        Console.WriteLine("::TOKENIZER::");
        Console.WriteLine(":::::::::::::");
        Console.WriteLine(tokens.Format());
    }
    public static void DebugPrint(this IExpression Function)
    {
        Console.WriteLine("\n::::::::::::");
        Console.WriteLine("::FUNCTION::");
        Console.WriteLine("::::::::::::");
        Console.WriteLine(Function.Format());
        Console.WriteLine(Function.ConvertToString());
        Function.printPrettyFunctionTree();
    }

    public static void printPrettyFunctionTree(this IExpression func)
    {
        var tree = new PrettyPrintTree<IExpression>(
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

    public static string ConvertToString(this IExpression func)
    {
        return func switch
        {
            Number n     => n.value.ToString(),
            Fraction n   => $"({n.numerator}/{n.denominator})",
            Symbols n    => n.identifier.ToString(),
            Product n    => $"*[{string.Join(", ", n.value.Select(n => n.ConvertToString()))}]",
            Sum n        => $"+[{string.Join(", ", n.value.Select(n => n.ConvertToString()))}]",
            Power n      => $"({n.Base.ConvertToString()}^{n.Exponent.ConvertToString()})",
            Function n   => $"{n.name}({n.value.ConvertToString()})",

            _ => throw new ArgumentOutOfRangeException(nameof(func)),
        };
    }
}