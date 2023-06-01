namespace nineT1CD;

public static class ToString
{
    public static string Repr(this List<Token> tokens)
    {
        return $"[ {String.Join(" , " , tokens.Select(t => t.ToString()))} ]";
    }
}

public class Derivate
{
    public static void run(string input)
    {
        var Lexer = new Lexer(input);
        var token = Lexer.parseText();
        Console.WriteLine(token.Repr());
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