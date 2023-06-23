namespace nineT1CD;
public static partial class Derivate
{
    public static void run(string input)
    {
        var Lexer = new Lexer(input);
        var token = Lexer.parseText();

        var Parser = new Parser(token);
        var expr = Parser.Parse();
        var function = new Derivative().dx(expr);

        var Evaluator = new Evaluator();
        var simplifiedExpr = Evaluator.eval(expr); 
        var simplifiedDx = Evaluator.eval(function); 

        Console.WriteLine();
        Console.WriteLine("TOKENIZER::");
        Console.WriteLine(token.Repr());
        Console.WriteLine("AST::");
        Console.WriteLine(new ASTPrint().print(expr));
        Console.WriteLine(new ASTPrint().print(simplifiedExpr));
        Console.WriteLine("Derivative::");
        Console.WriteLine(new FunctionPrint().print(function));
        Console.WriteLine(new FunctionPrint().print(simplifiedDx));
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