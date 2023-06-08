namespace nineT1CD;

public static class Debug
{
    public static string Repr(this List<Token> tokens)
    {
        return $"[ { String.Join(" , ", tokens.Select(t => t.ToString())) } ]";
    }
}