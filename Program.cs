namespace nineT1CD;
public static partial class Derivate
{
    public static void run(string input)
    {
        var Lexer = new Lexer(input);
        List<Token> token = Lexer.parseText();

        var Parser = new Parser(token);
        Node expr = Parser.Parse();

        var Evaluator = new Evaluator();
        Node simplifiedExpr = Evaluator.eval(expr); 

        var function = new Derivative().dx(expr);
        Node simplifiedDx = Evaluator.eval(function);
        
        Console.WriteLine();
        token.printTokens();
        expr.printAST(simplifiedExpr);
        function.printFunction(simplifiedDx);
        Console.WriteLine();

    }
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("derivate > ");
            string? input = Console.ReadLine();
            if (input == String.Empty || input == null) break;
            run(input);
        }
    }  
}