namespace nineT1CD;

public class ParseError : Exception 
{
    public ParseError(string? message) : base(message) {}
    public override string StackTrace
    {
        get { return String.Empty; }
    }
}
public static partial class Derivate
{
    public static void LexerError(string message, int position)
    {
        Console.WriteLine($"Error: {message} in position {position}. \n");
        Environment.Exit(0);
    }
    public static ParseError ParserError(string message, int position)
    {
        Console.WriteLine($"Error: {message} in position {position}. \n");
        return new ParseError(String.Empty);
    }
}