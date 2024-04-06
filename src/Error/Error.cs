namespace Derivate;

public class ParserError : Exception 
{
    public ParserError(string message) : base(message) {}
    public override string StackTrace { get { return string.Empty; } }
}
public class LexerError : Exception 
{
    public LexerError(string message) : base(message) {}
    public override string StackTrace { get { return string.Empty; } }
}

public static partial class Derivate
{
    public static void LexerError(string message, int position)
    {
        throw new LexerError($"{message} in position {position}.");
    }
    public static ParserError ParserError(string message, int position)
    {
        return new ParserError($"{message} in position {position}.");
    }
}