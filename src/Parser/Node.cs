using System.Diagnostics;

namespace Derivate;
public abstract record Node { 
    public abstract Token token { get; init; }
    public TokenType type { get => token.type; }
    public object value { get => token.value; }
    public void Deconstruct(out TokenType type) => type = this.type;

    public static Symbol f(string value) => new(value);
    public static Literal f(double value) => new(value);
    public static Unary f(Token token, Node right) => new(token, right);
    public static Binary f(Node left, Token token, Node right) => new(left, token, right);
}
public sealed record Symbol : Node
{
    public override Token token { get; init; }
    public Symbol(Token token)
    {
        Debug.Assert( token.type.match(TokenType.VAR) );
        this.token = token;
    }
    public new string value { get => token.value.ToString() ?? string.Empty; }
    public Symbol(string value): this(Token.VAR(value)) {}
    public void Deconstruct(out TokenType type, out string value) => (type, value) = (this.type, this.value);
}
public sealed record Literal: Node
{
    public override Token token { get; init; }
    public Literal(Token token)
    {
        Debug.Assert( token.type.match(TokenType.INT, TokenType.FLOAT, TokenType.CONST) );
        this.token = token;
    }
    public new double value { get => Convert.ToDouble(token.value); }
    public Literal(double value): this(
        value switch
        {
            Math.PI or Math.E => Token.CONST(value),
            var n when double.IsInteger(n) => Token.INT((int) n),
            _ => Token.FLOAT(value),
        }
    ) {}
    public void Deconstruct(out TokenType type, out double value) => (type, value) = (this.type, this.value);
}
public sealed record Unary : Node
{
    public Node right { get; init; }
    public override Token token { get; init; }
    public Unary(Token token, Node right)
    {
        this.token = token;
        this.right = right;
    }
    public void Deconstruct(out TokenType type, out Node right) => (type, right) = (this.type, this.right);
}
public sealed record Binary : Node
{
    public Node left { get; init; }
    public Node right { get; init; }
    public override Token token { get; init; }
    public Binary(Node left, Token token, Node right)
    {
        this.token = token;
        this.left = left;
        this.right = right;
    }
    public void Deconstruct(out Node left, out TokenType type, out Node right) 
        => (left, type, right) = (this.left, this.type, this.right);
}