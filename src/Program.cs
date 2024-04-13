namespace Derivate;
public static partial class Derivate
{
    public static void run(string input)
    {
        List<Token> token = Lexer.ParseText(input).ToList();
        token.DebugPrint();

        var Parser = new Parser(token);
        Node expr = Parser.Parse();
        expr.DebugPrint();

        Expression fx = expr.ToFunction().Simplify();
        fx.DebugPrint();

        Expression derivative = fx.Dx();
        derivative.DebugPrint();
    }
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("derivate > ");
            string? input = Console.ReadLine();
            if (input == string.Empty || input == null) break;
            run(input);
        }
    }  
}